using System.IO;

namespace Course3_2.Service
{
    public class Logger
    {
        private string _filepath;
        private bool _isInitialized;
        private FileStream _fileStream;

        public void Initialize()
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                _filepath = "~/logfiles/log.txt";
                if (!File.Exists(_filepath))
                {
                    File.Create(_filepath);
                }


                _fileStream = File.Open(_filepath, FileMode.Append);
            }
        }

        public void SetPath(string path)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (!path.Equals(_filepath))
            {
                _fileStream.Close();
                if (!File.Exists(path))
                {
                    File.Create(path);
                }

                _fileStream = File.Open(path, FileMode.Append);
            }
        }

        public void RewriteCurrent()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            _fileStream.Close();
            _fileStream = File.Open(_filepath, FileMode.Open);
        }
    }
}