using System;
using Xunit;

#pragma warning disable MaxPublicMethodCount // a public methods count maximum

namespace Xive.Test
{
    public sealed class MemoryPropsTests
    {
        [Fact]
        public void DeliversDefault()
        {
            Assert.Equal(
                "I MUST INSIST: I EXIST!",
                new RamProps()
                    .Value("not-existing", "I MUST INSIST: I EXIST!")
            );
        }

        [Fact]
        public void DeliversSingleValue()
        {
            Assert.Equal(
                "good boy!",
                new RamProps()
                    .Refined("behaviour", "good boy!")
                    .Value("behaviour")
            );
        }

        [Fact]
        public void OverwritesValue()
        {
            Assert.Equal(
                "average boy.",
                new RamProps()
                    .Refined("behaviour", "good boy!")
                    .Refined("behaviour", "average boy.")
                    .Value("behaviour")
            );
        }

        [Fact]
        public void DeliversMultipleValues()
        {
            Assert.Equal(
                "good boy, bad boy",
                new Yaapii.Atoms.Text.Joined(", ",
                    new RamProps()
                        .Refined("name", "Mr Jekyll/Mr. Hide")
                        .Refined("behaviour", "good boy", "bad boy")
                        .Values("behaviour")
                ).AsString()
            );
        }

        [Fact]
        public void AppliesPropsInput()
        {
            Assert.Equal(
                "",
                new RamProps()
                    .Refined("behaviour", "nasty boy")
                    .Refined(
                        new FkPropsInput(props => { props.Remove("behaviour"); return props; })
                    )
                    .Value("behaviour")
            );
        }

        [Fact]
        public void RejectsSingleWhenMultipleExist()
        {
            Assert.Throws<InvalidOperationException>(() =>
                new RamProps()
                    .Refined("name", "Mr Jekyll/Mr. Hide")
                    .Refined("behaviour", "good boy", "bad boy")
                    .Value("behaviour")
            );
        }
    }
}
