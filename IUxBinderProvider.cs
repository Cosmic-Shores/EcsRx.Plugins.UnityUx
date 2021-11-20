namespace EcsRx.Plugins.UnityUx {
    public interface IUxBinderProvider {
        IUxBinder<T> GetBinder<T>() where T : IUxComponent;
    }
}
