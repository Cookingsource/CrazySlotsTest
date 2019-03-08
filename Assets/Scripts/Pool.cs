namespace CrazySlots
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Pool<T> where T : MonoBehaviour 
    {
        private List<PoolItem> items;

        public void Initialize(int maxItems, T instanceToClone)
        {
            items = new List<PoolItem>(maxItems);
            for( int i = 0; i<maxItems; i++)
            {
                items.Add( new PoolItem()
                {
                    Value =  MonoBehaviour.Instantiate(instanceToClone),
                    IsRetained = false
                });
            }
        }

        public T Retain()
        {
            var itemToRetain = items.FirstOrDefault( i => !i.IsRetained);
            if(itemToRetain == null)
                return null;

            itemToRetain.IsRetained = true;
            return itemToRetain.Value;
        }

        public void Release(T item)
        {
            var itemToRelease = items.First( i => i.Value == item);
            if( itemToRelease == null)
                return;

            itemToRelease.IsRetained = false;
        }

        private class PoolItem
        {
            public T Value;
            public bool IsRetained;
        }
    }
}

