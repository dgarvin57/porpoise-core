using FluentAssertions;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Engines;

public class QuestionEngineUtilityTests
{
    [Fact]
    public void IsQuestionFilled_ShouldReturnTrue_WhenQuestionHasDataFileColAndQstNumber()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = 5,
            QstNumber = "Q1"
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsQuestionFilled_ShouldReturnTrue_WhenQuestionHasDataFileColAndQstLabel()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = 5,
            QstLabel = "Age"
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsQuestionFilled_ShouldReturnTrue_WhenQuestionHasBothQstNumberAndQstLabel()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = 5,
            QstNumber = "Q1",
            QstLabel = "Age"
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsQuestionFilled_ShouldReturnFalse_WhenDataFileColIsZero()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = 0,
            QstNumber = "Q1"
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsQuestionFilled_ShouldReturnFalse_WhenDataFileColIsNegative()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = -1,
            QstNumber = "Q1"
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsQuestionFilled_ShouldReturnFalse_WhenQstNumberAndQstLabelAreEmpty()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = 5,
            QstNumber = string.Empty,
            QstLabel = string.Empty
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsQuestionFilled_ShouldReturnFalse_WhenQstNumberAndQstLabelAreNull()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = 5,
            QstNumber = null,
            QstLabel = null
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsQuestionFilled_ShouldReturnFalse_WhenQstNumberAndQstLabelAreWhitespace()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = 5,
            QstNumber = "   ",
            QstLabel = "  "
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsQuestionFilled_ShouldThrowArgumentNullException_WhenQuestionIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => QuestionEngine.IsQuestionFilled(null!));
    }

    [Fact]
    public void GetQuestionsInBlock_ShouldReturnEmpty_WhenQuestionIsDiscreet()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion },
            new() { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion }
        };

        // Act
        var result = QuestionEngine.GetQuestionsInBlock(allQuestions[0], allQuestions);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetQuestionsInBlock_ShouldReturnSingleQuestion_WhenBlockHasOnlyFirstQuestion()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion }
        };

        // Act
        var result = QuestionEngine.GetQuestionsInBlock(allQuestions[0], allQuestions);

        // Assert
        result.Should().HaveCount(1);
        result[0].QstNumber.Should().Be("Q1");
    }

    [Fact]
    public void GetQuestionsInBlock_ShouldReturnAllQuestionsInBlock()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion },
            new() { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q3", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion },
            new() { QstNumber = "Q4", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion },
            new() { QstNumber = "Q5", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion }
        };

        // Act
        var result = QuestionEngine.GetQuestionsInBlock(allQuestions[2], allQuestions);

        // Assert
        result.Should().HaveCount(3);
        result.Select(q => q.QstNumber).Should().Equal("Q2", "Q3", "Q4");
    }

    [Fact]
    public void GetQuestionsInBlock_ShouldReturnBlockFromFirstQuestion()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion },
            new() { QstNumber = "Q3", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion }
        };

        // Act
        var result = QuestionEngine.GetQuestionsInBlock(allQuestions[0], allQuestions);

        // Assert
        result.Should().HaveCount(3);
        result.Select(q => q.QstNumber).Should().Equal("Q1", "Q2", "Q3");
    }

    [Fact]
    public void GetQuestionsInBlock_ShouldReturnBlockFromMiddleQuestion()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion },
            new() { QstNumber = "Q3", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion }
        };

        // Act
        var result = QuestionEngine.GetQuestionsInBlock(allQuestions[1], allQuestions);

        // Assert
        result.Should().HaveCount(3);
        result.Select(q => q.QstNumber).Should().Equal("Q1", "Q2", "Q3");
    }

    [Fact]
    public void GetQuestionsInBlock_ShouldReturnBlockFromLastQuestion()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion },
            new() { QstNumber = "Q3", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion }
        };

        // Act - call from last question (continuation)
        var result = QuestionEngine.GetQuestionsInBlock(allQuestions[2], allQuestions);

        // Assert
        result.Should().HaveCount(3);
        result.Select(q => q.QstNumber).Should().Equal("Q1", "Q2", "Q3");
    }

    [Fact]
    public void GetQuestionsInBlock_ShouldHandleMultipleBlocksInSurvey()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion },
            new() { QstNumber = "Q3", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion },
            new() { QstNumber = "Q4", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q5", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion },
            new() { QstNumber = "Q6", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion }
        };

        // Act - get second block from middle
        var result = QuestionEngine.GetQuestionsInBlock(allQuestions[4], allQuestions);

        // Assert
        result.Should().HaveCount(3);
        result.Select(q => q.QstNumber).Should().Equal("Q4", "Q5", "Q6");
    }

    [Fact]
    public void GetQuestionsInBlock_ShouldStopAtNextFirstQuestion()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion },
            new() { QstNumber = "Q3", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q4", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion }
        };

        // Act - get first block
        var result = QuestionEngine.GetQuestionsInBlock(allQuestions[1], allQuestions);

        // Assert
        result.Should().HaveCount(2);
        result.Select(q => q.QstNumber).Should().Equal("Q1", "Q2");
    }

    [Fact]
    public void GetQuestionsInBlock_ShouldHandleBlockAtEndOfList()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion },
            new() { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q3", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion },
            new() { QstNumber = "Q4", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion }
        };

        // Act
        var result = QuestionEngine.GetQuestionsInBlock(allQuestions[3], allQuestions);

        // Assert
        result.Should().HaveCount(3);
        result.Select(q => q.QstNumber).Should().Equal("Q2", "Q3", "Q4");
    }

    [Fact]
    public void SetQuestionModified_ShouldUpdateModifiedFields_WhenQuestionIsDirty()
    {
        // Arrange
        var question = new Question { QstNumber = "Q1" };
        question.QstLabel = "Modified"; // Makes it dirty
        var questions = new List<Question> { question };

        // Act
        QuestionEngine.SetQuestionModified(questions);

        // Assert
        question.ModifiedBy.Should().Be(Environment.UserName);
        question.ModifiedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SetQuestionModified_ShouldNotUpdate_WhenQuestionIsClean()
    {
        // Arrange
        var question = new Question { QstNumber = "Q1" };
        question.MarkClean();
        var questions = new List<Question> { question };
        var originalModifiedBy = question.ModifiedBy;
        var originalModifiedOn = question.ModifiedOn;

        // Act
        QuestionEngine.SetQuestionModified(questions);

        // Assert
        question.ModifiedBy.Should().Be(originalModifiedBy);
        question.ModifiedOn.Should().Be(originalModifiedOn);
    }

    [Fact]
    public void SetQuestionModified_ShouldUpdateResponseModifiedFields_WhenResponseIsDirty()
    {
        // Arrange
        var question = new Question { QstNumber = "Q1" };
        var response = new Response { RespValue = 1, Label = "Yes" };
        question.Responses.Add(response);
        question.MarkClean();
        response.Label = "Modified"; // Makes response dirty
        var questions = new List<Question> { question };

        // Act
        QuestionEngine.SetQuestionModified(questions);

        // Assert
        response.ModifiedBy.Should().Be(Environment.UserName);
        response.ModifiedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SetQuestionModified_ShouldHandleMultipleQuestions()
    {
        // Arrange
        var q1 = new Question { QstNumber = "Q1" };
        var q2 = new Question { QstNumber = "Q2" };
        q1.QstLabel = "Dirty1";
        q2.QstLabel = "Dirty2";
        var questions = new List<Question> { q1, q2 };

        // Act
        QuestionEngine.SetQuestionModified(questions);

        // Assert
        q1.ModifiedBy.Should().Be(Environment.UserName);
        q2.ModifiedBy.Should().Be(Environment.UserName);
    }

    [Fact]
    public void DefaultResponseIndex_ShouldSetNeutral_WhenIndexTypeIsNone()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock },
            new() { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion }
        };
        allQuestions[0].Responses.Add(new Response { RespValue = 1, IndexType = ResponseIndexType.None });
        allQuestions[0].Responses.Add(new Response { RespValue = 2, IndexType = ResponseIndexType.Positive });
        allQuestions[1].Responses.Add(new Response { RespValue = 1, IndexType = ResponseIndexType.None });

        // Act
        QuestionEngine.DefaultResponseIndex(allQuestions[0], allQuestions);

        // Assert
        allQuestions[0].Responses[0].IndexType.Should().Be(ResponseIndexType.Neutral);
        allQuestions[0].Responses[1].IndexType.Should().Be(ResponseIndexType.Positive); // Unchanged
        allQuestions[1].Responses[0].IndexType.Should().Be(ResponseIndexType.Neutral);
    }

    [Fact]
    public void DefaultResponseIndex_ShouldHandleDiscreetQuestion()
    {
        // Arrange
        var allQuestions = new ObjectListBase<Question>
        {
            new() { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion }
        };
        allQuestions[0].Responses.Add(new Response { RespValue = 1, IndexType = ResponseIndexType.None });

        // Act
        QuestionEngine.DefaultResponseIndex(allQuestions[0], allQuestions);

        // Assert - discreet questions return empty block, so nothing changes
        allQuestions[0].Responses[0].IndexType.Should().Be(ResponseIndexType.None);
    }

    [Fact]
    public void IsIVInSameBlockAsDV_ShouldReturnTrue_WhenInSameBlock()
    {
        // Arrange
        var dv = new Question { Id = Guid.NewGuid(), QstNumber = "DV", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock };
        var iv = new Question { Id = Guid.NewGuid(), QstNumber = "IV", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion };
        var allQuestions = new ObjectListBase<Question> { dv, iv };

        // Act
        var result = QuestionEngine.IsIVInSameBlockAsDV(dv, iv, allQuestions);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIVInSameBlockAsDV_ShouldReturnFalse_WhenInDifferentBlocks()
    {
        // Arrange
        var dv = new Question { Id = Guid.NewGuid(), QstNumber = "DV", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock };
        var otherBlockStart = new Question { Id = Guid.NewGuid(), QstNumber = "Other", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion };
        var iv = new Question { Id = Guid.NewGuid(), QstNumber = "IV", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock };
        var allQuestions = new ObjectListBase<Question> { dv, otherBlockStart, iv };

        // Act
        var result = QuestionEngine.IsIVInSameBlockAsDV(dv, iv, allQuestions);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsIVInSameBlockAsDV_ShouldReturnFalse_WhenDVIsDiscreet()
    {
        // Arrange
        var dv = new Question { Id = Guid.NewGuid(), QstNumber = "DV", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion };
        var iv = new Question { Id = Guid.NewGuid(), QstNumber = "IV", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion };
        var allQuestions = new ObjectListBase<Question> { dv, iv };

        // Act
        var result = QuestionEngine.IsIVInSameBlockAsDV(dv, iv, allQuestions);

        // Assert
        result.Should().BeFalse();
    }
}
