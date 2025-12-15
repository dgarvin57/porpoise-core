using System;
using System.Collections.Generic;
using FluentAssertions;
using Porpoise.Api.Services;
using Xunit;

namespace Porpoise.Api.Tests.Services;

public class PromptTemplateEngineTests
{
    private readonly PromptTemplateEngine _engine;

    public PromptTemplateEngineTests()
    {
        _engine = new PromptTemplateEngine();
    }

    #region Simple Replacement Tests

    [Fact]
    public void Render_ReplacesSimplePlaceholder_WithContextValue()
    {
        // Arrange
        var template = "Hello {{name}}!";
        var context = new Dictionary<string, object>
        {
            { "name", "Alice" }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Hello Alice!");
    }

    [Fact]
    public void Render_ReplacesMultiplePlaceholders()
    {
        // Arrange
        var template = "{{greeting}} {{name}}, you have {{count}} messages.";
        var context = new Dictionary<string, object>
        {
            { "greeting", "Hello" },
            { "name", "Bob" },
            { "count", 5 }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Hello Bob, you have 5 messages.");
    }

    [Fact]
    public void Render_KeepsPlaceholder_WhenKeyNotFound()
    {
        // Arrange
        var template = "Hello {{name}}, welcome to {{place}}!";
        var context = new Dictionary<string, object>
        {
            { "name", "Charlie" }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Hello Charlie, welcome to {{place}}!");
    }

    [Fact]
    public void Render_HandlesNullValue_AsEmptyString()
    {
        // Arrange
        var template = "Value: {{value}}";
        var context = new Dictionary<string, object>
        {
            { "value", null! }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Value: ");
    }

    [Fact]
    public void Render_HandlesEmptyTemplate()
    {
        // Arrange
        var template = "";
        var context = new Dictionary<string, object>
        {
            { "key", "value" }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void Render_HandlesTemplateWithNoPlaceholders()
    {
        // Arrange
        var template = "This is plain text with no placeholders.";
        var context = new Dictionary<string, object>
        {
            { "key", "value" }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("This is plain text with no placeholders.");
    }

    [Fact]
    public void Render_HandlesNumericValues()
    {
        // Arrange
        var template = "Count: {{count}}, Price: {{price}}";
        var context = new Dictionary<string, object>
        {
            { "count", 42 },
            { "price", 19.99 }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Count: 42, Price: 19.99");
    }

    #endregion

    #region Each Block Tests

    [Fact]
    public void Render_ProcessesEachBlock_WithDictionaryItems()
    {
        // Arrange
        var template = "Items:\n{{#each items}}- {{name}}: {{value}}\n{{/each}}";
        var context = new Dictionary<string, object>
        {
            {
                "items", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "name", "Apple" }, { "value", "Red" } },
                    new Dictionary<string, object> { { "name", "Banana" }, { "value", "Yellow" } }
                }
            }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Items:\n- Apple: Red\n- Banana: Yellow\n");
    }

    [Fact]
    public void Render_ProcessesEachBlock_WithObjectItems()
    {
        // Arrange
        // Properties are converted to camelCase: Name -> name, Age -> age
        var template = "{{#each people}}{{name}} is {{age}} years old.\n{{/each}}";
        var people = new List<Person>
        {
            new Person { Name = "Alice", Age = 30 },
            new Person { Name = "Bob", Age = 25 }
        };
        var context = new Dictionary<string, object>
        {
            { "people", people }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Alice is 30 years old.\nBob is 25 years old.\n");
    }

    [Fact]
    public void Render_HandlesEmptyCollection_InEachBlock()
    {
        // Arrange
        var template = "Items:\n{{#each items}}- {{name}}\n{{/each}}Done.";
        var context = new Dictionary<string, object>
        {
            { "items", new List<Dictionary<string, object>>() }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Items:\nDone.");
    }

    [Fact]
    public void Render_HandlesNullItem_InEachBlock()
    {
        // Arrange
        var template = "{{#each items}}{{name}}\n{{/each}}";
        var context = new Dictionary<string, object>
        {
            { "items", new List<object?> { null, new { name = "Item" }, null } }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Contain("Item");
    }

    [Fact]
    public void Render_ReturnsEmpty_WhenCollectionNotFound()
    {
        // Arrange
        var template = "{{#each items}}{{name}}{{/each}}";
        var context = new Dictionary<string, object>();

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void Render_ProcessesString_AsCharacterCollection()
    {
        // Arrange
        // Note: String is IEnumerable<char>, so each character is processed
        var template = "{{#each items}}X{{/each}}";
        var context = new Dictionary<string, object>
        {
            { "items", "abc" }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("XXX"); // 3 characters = 3 X's
    }

    [Fact]
    public void Render_HandlesMultipleEachBlocks()
    {
        // Arrange
        var template = "Colors:\n{{#each colors}}{{name}} {{/each}}\nNumbers:\n{{#each numbers}}{{value}} {{/each}}";
        var context = new Dictionary<string, object>
        {
            {
                "colors", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "name", "Red" } },
                    new Dictionary<string, object> { { "name", "Blue" } }
                }
            },
            {
                "numbers", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "value", "1" } },
                    new Dictionary<string, object> { { "value", "2" } }
                }
            }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Colors:\nRed Blue \nNumbers:\n1 2 ");
    }

    #endregion

    #region Combined Tests

    [Fact]
    public void Render_CombinesSimpleReplacementAndEachBlock()
    {
        // Arrange
        var template = "User: {{username}}\nItems:\n{{#each items}}- {{name}}\n{{/each}}";
        var context = new Dictionary<string, object>
        {
            { "username", "testuser" },
            {
                "items", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "name", "Item1" } },
                    new Dictionary<string, object> { { "name", "Item2" } }
                }
            }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Contain("User: testuser");
        result.Should().Contain("- Item1");
        result.Should().Contain("- Item2");
    }

    [Fact]
    public void Render_HandlesComplexRealWorldTemplate()
    {
        // Arrange
        var template = @"Survey: {{surveyName}}
Total Questions: {{totalQuestions}}
Questions:
{{#each questions}}
  Q{{number}}: {{text}} (Type: {{type}})
{{/each}}
Status: {{status}}";

        var context = new Dictionary<string, object>
        {
            { "surveyName", "Customer Satisfaction" },
            { "totalQuestions", 3 },
            {
                "questions", new List<Dictionary<string, object>>
                {
                    new Dictionary<string, object> { { "number", "1" }, { "text", "How satisfied are you?" }, { "type", "Rating" } },
                    new Dictionary<string, object> { { "number", "2" }, { "text", "Would you recommend?" }, { "type", "Yes/No" } },
                    new Dictionary<string, object> { { "number", "3" }, { "text", "Comments" }, { "type", "Text" } }
                }
            },
            { "status", "Active" }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Contain("Survey: Customer Satisfaction");
        result.Should().Contain("Total Questions: 3");
        result.Should().Contain("Q1: How satisfied are you? (Type: Rating)");
        result.Should().Contain("Q2: Would you recommend? (Type: Yes/No)");
        result.Should().Contain("Q3: Comments (Type: Text)");
        result.Should().Contain("Status: Active");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Render_HandlesNestedBraces_OutsidePlaceholders()
    {
        // Arrange
        var template = "Code: { {{value}} }";
        var context = new Dictionary<string, object>
        {
            { "value", "test" }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Code: { test }");
    }

    [Fact]
    public void Render_HandlesEmptyContext()
    {
        // Arrange
        var template = "Static text without placeholders";
        var context = new Dictionary<string, object>();

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("Static text without placeholders");
    }

    [Fact]
    public void Render_ConvertsPascalCaseToCamelCase_InObjectProperties()
    {
        // Arrange
        var template = "{{#each items}}{{name}}-{{value}} {{/each}}";
        var items = new List<TestItem>
        {
            new TestItem { Name = "First", Value = 1 },
            new TestItem { Name = "Second", Value = 2 }
        };
        var context = new Dictionary<string, object>
        {
            { "items", items }
        };

        // Act
        var result = _engine.Render(template, context);

        // Assert
        result.Should().Be("First-1 Second-2 ");
    }

    #endregion

    // Test helper classes
    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class TestItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
