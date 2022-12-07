using DefaultNamespace;
using Unity.VisualScripting;
using UnityEngine;

namespace Core
{
    public static class BuffSystem
    {
        public static Buff AddBuff<T>(this Character target, BuffType buffType, float duration = -1) where T : Buff
        {
            if (target && target.BuffSocket.TryGetComponent(out T buff))
            {
                if (buff.IsBuffActivated)
                {
                    buff.OnOverride(duration);
                    return buff;
                }
            }

            buff = target.BuffSocket.AddComponent<T>();
            buff.BuffType = buffType;
            buff.OnAddBuff(target, duration);
            return buff;
        }
    }
}