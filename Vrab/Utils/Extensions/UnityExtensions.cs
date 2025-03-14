using UnityEngine;
using System;
using Unity;
using System.Linq;
using R2API.Networking.Interfaces;

namespace Vrab.Utils
{
    public static class UnityExtensions
    {
        public static void RemoveComponent<T>(this GameObject self) where T : Component
        {
            Object.Destroy(self.GetComponent<T>());
        }

        public static void RemoveComponents<T>(this GameObject self) where T : Component
        {
            T[] coms = self.GetComponents<T>();
            for (int i = 0; i < coms.Length; i++)
            {
                Object.Destroy(coms[i]);
            }
        }

        public static void CallNetworkedMethod(this GameObject self, string method, R2API.Networking.NetworkDestination dest = R2API.Networking.NetworkDestination.Clients) {
            new CallNetworkedMethod(self, method).Send(dest);
        }

        public static T FindComponent<T>(this GameObject self, string name) where T : Component {
            return self.GetComponentsInChildren<T>().FirstOrDefault(x => x.gameObject.name == name);
        }

        public static void RemoveComponent<T>(this Component self) where T : Component
        {
            Object.Destroy(self.GetComponent<T>());
        }

        public static void RemoveComponents<T>(this Component self) where T : Component
        {
            T[] coms = self.GetComponents<T>();
            for (int i = 0; i < coms.Length; i++)
            {
                Object.Destroy(coms[i]);
            }
        }

        public static T AddComponent<T>(this Component self) where T : Component
        {
            return self.gameObject.AddComponent<T>();
        }

        public static Sprite MakeSprite(this Texture2D self)
        {
            return Sprite.Create(self, new(0, 0, self.width, self.height), new(0.5f, 0.5f), 100f);
        }
    }
}