namespace Osmy.Gui.Views
{
    public interface ICloseable<T>
    {
        void CloseWithResult(T result);
    }
}
