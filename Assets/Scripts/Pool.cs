namespace CrazySlots
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [Serializable]
    public class Pool<T> where T : MonoBehaviour 
    {
        private List<PoolItem> items;
        public T instanceToClone;

        public void Initialize(int maxItems)
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

            Release(itemToRelease);
        }

        public void ReleaseAll()
        {
            foreach( var item in items)
                Release(item);
        }

        private void Release( PoolItem item)
        {
            item.IsRetained = false;
            item.Value.transform.position = instanceToClone.transform.position;
        }

        private class PoolItem
        {
            public T Value;
            public bool IsRetained;
        }
    }
}

