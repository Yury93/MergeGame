using System;

namespace Menu.MergeGame
{ 
    public interface IResource
    {
        public abstract Type ResourceType { get; }
        public abstract bool HasResource(int count);

        public abstract void TakeResource(int count);

        public abstract void AddResource(int count); 
    }
}