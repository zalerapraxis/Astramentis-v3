using System;
using System.Collections.Generic;
using System.Text;

namespace Astramentis.Attributes
{
    // show examples of how to use commands
    // used by help module to show command usage for commands with parameters
    internal sealed class ExampleAttribute : Attribute
    {
        public string ExampleText { get; private set; }

        public ExampleAttribute(string exampleText)
        {
            ExampleText = exampleText;
        }
    }
}
