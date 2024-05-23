using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunit;

/// <summary>
/// Allows deeper investigation of Exceptions.
/// </summary>
public static class AssertException
{
    /// <summary>
    /// Asserts that exception type matches and message equals given one.
    /// </summary>
    public static void MessageMatches<Ex>(string message, Action act)
    {
        try { act(); }
        catch (Exception ex)
        {
            Assert.IsType<Ex>(ex);
            Assert.Equal(ex.Message, message);
        }
    }

    /// <summary>
    /// Asserts that exception type matches and message starts with given text.
    /// </summary>
    public static void MessageStartsWith<Ex>(string message, Action act)
    {
        try { act(); }
        catch (Exception ex)
        {
            Assert.IsType<Ex>(ex);
            Assert.StartsWith(message, ex.Message);
        }
    }

    /// <summary>
    /// Asserts that exception type matches and message contains given text.
    /// </summary>
    public static void MessageContains<Ex>(string message, Action act)
    {
        try { act(); }
        catch (Exception ex)
        {
            Assert.IsType<Ex>(ex);
            Assert.Contains(message, ex.Message);
        }
    }
}
