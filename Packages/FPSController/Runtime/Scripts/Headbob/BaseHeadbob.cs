using System;

namespace Headbob
{
    [Serializable]
    public abstract class BaseHeadbob
    {
        public Action OnHit;
        
        public virtual void Initialize() {}
        
        public virtual void Update() {}
    }
}