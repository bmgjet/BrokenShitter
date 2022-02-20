using UnityEngine;
using CompanionServer.Handlers;
using ProtoBuf;

namespace Oxide.Plugins
{
    [Info("BrokenShitter", "bmgjet", "0.0.1")]
    [Description("Functions for Broken Shitter.")]

    public class BrokenShitter : RustPlugin
    {
        public static float Damage = 25f;
        public static float SinkDistance = 3f;
        public static Vector3 PrefabCenter = new Vector3(0, 0, 0);
        Timer dungstream;

        void OnServerInitialized()
        {
            foreach (PrefabData pd in World.Serialization.world.prefabs)
            {
                if (pd.category.Contains("BrokenShitter") && pd.id == 1753929286)
                {
                    Puts("Found Monument Center");
                    PrefabCenter = pd.position;
                    dungstream = timer.Repeat(2, 0, () =>
                    {
                        BaseEntity poop = DropNearPosition(Spawnpoop(), pd.position);
                        ApplyVelocity(poop);
                        timer.Once(4, () =>
                        {
                            try { poop.Kill(); } catch { }
                        });
                    });
                }
            }
            foreach (BasePlayer current in BasePlayer.activePlayerList)
            {
                AddStinkCheck(current);
            }
        }

        private void OnPlayerSleepEnded(BasePlayer current)
        {
            AddStinkCheck(current);
        }

        void Unload()
        {
            if (dungstream != null)
            {
                dungstream.Destroy();
            }
            var objects = GameObject.FindObjectsOfType(typeof(PoopStink));
            if (objects != null)
            {
                foreach (var gameObj in objects)
                {
                    GameObject.Destroy(gameObj);
                }
            }
        }

        void AddStinkCheck(BasePlayer player)
        {
            if (player.GetComponent<PoopStink>() == null && !player.IsNpc)
            {
                player.gameObject.AddComponent<PoopStink>();
            }
        }

        Item Spawnpoop()
        {
            return ItemManager.CreateByName("horsedung", 1);
        }

        BaseEntity DropNearPosition(Item item, Vector3 pos) => item.CreateWorldObject(pos);

        BaseEntity ApplyVelocity(BaseEntity entity)
        {
            entity.SetVelocity(new Vector3(Random.Range(-4f, 8f), Random.Range(-0.3f, 8f), Random.Range(-4f, 8f)));
            entity.SetAngularVelocity(new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f)));
            entity.SendNetworkUpdateImmediate();
            return entity;
        }

        private class PoopStink : FacepunchBehaviour
        {
            private BasePlayer _player;
            private void Awake()
            {
                _player = GetComponent<BasePlayer>();
                InvokeRepeating(Check, 4f, 4f);
            }

            private void Check()
            {
                if (Vector3.Distance(PrefabCenter, _player.transform.position) < SinkDistance)
                {
                    if (!_player.IsAlive() || !_player.IsConnected)
                        return;

                    Effect.server.Run("assets/bundled/prefabs/fx/gestures/drink_vomit.prefab", _player.transform.position);
                    _player.Hurt(Damage, Rust.DamageType.Heat, null, true);
                }
            }
        }
    }
}