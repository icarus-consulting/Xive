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
using Yaapii.Xambly;

namespace Xive.Xocument.Test
{
    public sealed class RamXocumentTests
    {
        [Fact]
        public void HasRootNode()
        {
            var xoc = new RamXocument("catalog.xml");
            Assert.Equal(1, xoc.Nodes("/catalog").Count);
        }

        [Fact]
        public void RootNodeIsCaseInsensitive()
        {
            var xoc = new RamXocument("TheXoc.xml");
            Assert.Equal(1, xoc.Nodes("/thexoc").Count);
        }

        [Fact]
        public void UpdatesXocument()
        {
            var xoc = new RamXocument("xoc");
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

        [Fact]
        public void ReturnsValues()
        {
            var xoc = new RamXocument("xoc");
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
