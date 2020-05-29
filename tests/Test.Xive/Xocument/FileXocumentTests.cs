//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Yaapii.Atoms.IO;
using Yaapii.Xambly;

namespace Xive.Xocument.Test
{
    public sealed class FileXocumentTests
    {
        [Fact]
        public void HasRootNode()
        {
            using (var temp = new TempDirectory())
            {
                var xoc = new FileXocument(temp.Value().FullName + "/catalog.xml");
                Assert.Equal(1, xoc.Nodes("/catalog").Count);
            }
        }

        [Fact]
        public void RootNodeIsCaseSensitive()
        {
            using (var temp = new TempDirectory())
            {
                var xoc = new FileXocument(temp.Value().FullName + "/TheXoc.xml");
                Assert.Equal(1, xoc.Nodes("/TheXoc").Count);
            }
        }

        [Fact]
        public void UpdatesXocument()
        {
            using (var temp = new TempDirectory())
            {
                var xoc = new FileXocument(temp.Value().FullName + "/xoc.xml");
                xoc.Modify(new Directives()
                .Xpath("/xoc")
                .Add("aNode")
                .Set("a value")
            );
                Assert.Equal(
                    "a value",
                    xoc.Value("/xoc/aNode/text()", "")
                );
            }
        }

        [Fact]
        public void ReturnsValues()
        {
            using (var temp = new TempDirectory())
            {
                var xoc = new FileXocument(temp.Value().FullName + "/xoc.xml");
                xoc.Modify(new Directives()
                .Xpath("/xoc")
                .Add("aNode")
                .Set("a value")
                .Up()
                .Add("aNode")
                .Set("another value")
            );
                Assert.Equal(
                    2,
                    xoc.Values("/xoc/aNode/text()").Count
                );
            }
        }
    }
}
