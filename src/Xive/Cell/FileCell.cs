using System;
using System.IO;
using Yaapii.Atoms;
using Yaapii.Atoms.IO;
using Yaapii.Atoms.Scalar;
using Yaapii.Atoms.Text;

namespace Xive.Cell
{
    /// <summary>
    /// A cell which exists physically as a file.
    /// </summary>
    public sealed class FileCell : ICell
    {
        private readonly IScalar<string> path;

        /// <summary>
        /// A cell which exists physically as a file.
        /// </summary>
        public FileCell(string root, string name) : this(
            new StickyScalar<string>(() => 
                Path.Combine(root, new StrictCellName(name).AsString())
            )
        )
        { }

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
            this.path =
                new SolidScalar<string>(() =>
                {
                    Validate(new Coordinate(path.Value()).AsString());
                    return path.Value();
                });
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
            if (!Path.IsPathRooted(path))
            {
                throw new ArgumentException($"Cannot work with path '{path}' because it is not rooted.");
            }
            if (Path.GetFileName(path) == String.Empty)
            {
                throw new ArgumentException($"Cannot work with path '{path}' because it is not a file.");
            }

            var dir = Path.GetDirectoryName(path);
            var file = Path.GetFileName(path);
            var invalidPathChars = Path.GetInvalidPathChars();
            var invalid = false;
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                if (file.Contains(c + ""))
                {
                    invalid = true;
                    break;
                }
            }
            if (invalid)
            {
                throw
                    new ArgumentException(
                        new FormattedText(
                            $"Cannot work with path '{path}' because file name contains invalid characters. Not allowed characters are: {0}",
                            String.Join(", ", Path.GetInvalidFileNameChars())
                        ).AsString()
                    );
            }

            invalid = false;
            foreach (var c in Path.GetInvalidPathChars())
            {
                if (dir.Contains(c + ""))
                {
                    invalid = true;
                    break;
                }
            }
            if (invalid)
            {
                throw
                    new ArgumentException(
                        new FormattedText(
                            $"Cannot work with path '{path}' because directory name contains invalid characters. Not allowed characters are: {0}",
                            Path.GetInvalidPathChars()
                        ).AsString()
                    );
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
