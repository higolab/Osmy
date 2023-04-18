using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Osmy.Core.Data
{
    /// <summary>
    /// 変更通知オブジェクト
    /// </summary>
    public class NotificationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// <paramref name="store"/>に<paramref name="value"/>を代入し，変更通知を送出します．
        /// <paramref name="store"/>と<paramref name="value"/>が等値であれば代入は行わず，変更通知も送出しません．
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="store"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns>値を代入し変更通知を送出した場合は<see langword="true"/>，それ以外は<see langword="false"/></returns>
        protected bool SetProperty<T>(ref T store, T value, [CallerMemberName] string propertyName = "")
        {
            if (Equals(store, value))
            {
                return false;
            }

            store = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
