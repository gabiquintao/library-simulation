using System.Text;

namespace Library.Core.Storage
{
    public sealed class TextStore
    {
        private byte[] _data = new byte[1024];
        private int _length;

        public (int Offset, int Length) Add(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            if (_length + bytes.Length > _data.Length)
                Array.Resize(ref _data, Math.Max(_data.Length * 2, _length + bytes.Length));

            bytes.CopyTo(_data, _length);
            int offset = _length;
            _length += bytes.Length;
            return (offset, bytes.Length);
        }

        public string Get(int offset, int length)
            => Encoding.UTF8.GetString(_data, offset, length);
    }
}
