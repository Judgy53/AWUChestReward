using BepInEx;
using RoR2;
using RoR2.Hologram;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace AWUChestReward
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    public class AWUChestRewardPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Judgy";
        public const string PluginName = "AWUChestReward";
        public const string PluginVersion = "1.0.0";

        public static string PluginDirectory { get; private set; }

        private readonly GameObject _chestPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/GoldChest/GoldChest.prefab").WaitForCompletion();
        private readonly GameObject _fxPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LightningStrikeOnHit/SimpleLightningStrikeImpact.prefab").WaitForCompletion();
        
        private float _diffCoefficientStageStart;
        private GameObject _holderGO;
        private bool _spawnInProgress = false;
        

        public void Awake()
        {
            PluginDirectory = System.IO.Path.GetDirectoryName(Info.Location);

            Log.Init(Logger);
            AWUConfig.Init(Config);

            On.RoR2.BossGroup.DropRewards += BossGroup_DropRewards;
            On.RoR2.Stage.Start += Stage_Start;
        }

        private IEnumerator Stage_Start(On.RoR2.Stage.orig_Start orig, Stage self)
        {
            if (Run.instance)
            {
                _diffCoefficientStageStart = Run.instance.difficultyCoefficient;
                _holderGO = null;
            }

            return orig(self);
        }

        private void BossGroup_DropRewards(On.RoR2.BossGroup.orig_DropRewards orig, BossGroup self)
        {
            if (AWUConfig.Enabled.Value 
                && self.name == "SuperRoboBallEncounter"
                && SceneManager.GetActiveScene().name == "shipgraveyard") // Scene check is probably unnecessary
            {
                StartCoroutine(SpawnCustomRewards());
                return;
            }

            orig(self);
        }

        private IEnumerator SpawnCustomRewards()
        {
            if (!NetworkServer.active)
                yield break;

            if (!Run.instance)
                yield break;

            if (_holderGO)
                yield break;

            if(_spawnInProgress)
                yield break;

            _spawnInProgress = true;

            _holderGO = new GameObject("HOLDER: AWU Chests");
            _holderGO.transform.position = new Vector3(34.699f, -3.1f, -39.428f);
            _holderGO.transform.eulerAngles = new Vector3(2.6f, 0f, 0f);

            var chestCount = 1;
            if (AWUConfig.ScaleWithPlayerCount.Value == true)
                chestCount = Run.instance.participatingPlayerCount;

            var anglePerChest = 360f / chestCount;
            var posOffset = new Vector3(4.888f, 0f, -4.892f);
            var chestCost = ComputeChestCost();

            for (int i = 0; i < chestCount; i++)
            {
                yield return new WaitForSeconds(.75f);

                var angle = anglePerChest * i;
                GameObject chestGO = Instantiate(_chestPrefab, _holderGO.transform);
                chestGO.transform.localPosition = Quaternion.AngleAxis(angle, Vector3.up) * posOffset;
                chestGO.transform.localEulerAngles = new Vector3(0f, (135f + angle) % 360f, 0f);
                NetworkServer.Spawn(chestGO);

                PurchaseInteraction purchaseComponent = chestGO.GetComponent<PurchaseInteraction>();
                purchaseComponent.Networkcost = chestCost;
                if (chestCost == 0)
                    chestGO.GetComponent<HologramProjector>().enabled = false; // Hack to avoid "25$" hologram on 0 cost chest

                var fxEffectData = new EffectData()
                {
                    origin =  chestGO.transform.position,
                };
                EffectManager.SpawnEffect(_fxPrefab,fxEffectData, true); // TODO: Add sound effect
            }

            _spawnInProgress = false;
        }

        private int ComputeChestCost()
        {
            return AWUConfig.ChestCostScalingMode.Value switch
            {
                AWUConfig.ScalingMode.None => AWUConfig.ChestCost.Value,
                AWUConfig.ScalingMode.StageStart => Run.instance.GetDifficultyScaledCost(AWUConfig.ChestCost.Value, _diffCoefficientStageStart),
                AWUConfig.ScalingMode.OnKill => Run.instance.GetDifficultyScaledCost(AWUConfig.ChestCost.Value, Run.instance.difficultyCoefficient),
                _ => throw new NotImplementedException("ChestCostScalingMode Unknown Value"),
            };
        }

#if DEBUG
        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.F2) && !_spawnInProgress)
            {
                if (_holderGO)
                {
                    Destroy(_holderGO);
                    _holderGO = null;
                }

                StartCoroutine(SpawnCustomRewards());
            }
        }
#endif
    }
}
