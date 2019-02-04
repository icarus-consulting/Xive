using System;
using System.IO;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Cell
{
    public sealed class FileCell : ICell
    {
        private readonly IScalar<string> path;

        /// <summary>
        /// A cell which exists physically as a file.
        /// </summary>
        public FileCell(string path) : this(new ScalarOf<string>(path))
        { }

        /// <summary>
        /// A cell which exists physically as a file.
        /// </summary>
        private FileCell(IScalar<string> path)
        {
            lock (this) //make path validation solid. Otherwise odd behaviour occures because creation can be left unfinished.
            {
                this.path =
                    new SolidScalar<string>(() =>
                    {
                        Validate(new Coordinate(path.Value()).AsString());
                        return path.Value();
                    });
            }
        }

        public byte[] Content()
        {
            byte[] result = new byte[0];
            if (File.Exists(this.path.Value()))
            {
                result =
                    new BytesOf(
                        new InputOf(new Uri(this.path.Value()))
                    ).AsBytes();
            }
            return result;
        }

        public void Update(IInput content)
        {
            if (content.Stream().Length == 0)
            {
                Cleanup();
            }
            else
            {
                Write(content);
            }
        }

        private void Validate(string path)
        {
            bool invalid;
            try
            {
                invalid =
                    !Path.IsPathRooted(path)
                    || Path.GetFileName(path) == String.Empty;
            }
            catch (Exception)
            {
                throw 
                    new ArgumentException(
                        new FormattedText(
                            "The filename contains invalid characters. Not allowed characters are: {0}",
                            Path.GetInvalidFileNameChars()
                        ).AsString()
                );
            }
            if (invalid)
            {
                throw new ArgumentException("Invalid filepath");
            }
        }

        private void Write(IInput content)
        {
            lock (this)
            {
                var dir = Path.GetDirectoryName(this.path.Value());
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllBytes(
                    this.path.Value(),
                    new BytesOf(content).AsBytes()
                );
            }
        }

        private void Cleanup()
        {
            lock (this)
            {
                if (File.Exists(this.path.Value()))
                {
                    File.Delete(this.path.Value());
                }
            }
        }

        public void Dispose()
        {

        }
    }
}
