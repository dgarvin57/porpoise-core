using FluentAssertions;
using Porpoise.Core.Engines;
using Porpoise.Core.Models;

namespace Porpoise.Core.Tests.Engines;

public class QuestionEngineBlockTests
{
    [Fact]
    public void GetQuestionsInBlock_ReturnsEmptyForDiscreetQuestion()
    {
        // Arrange
        var question = new Question
        {
            QstNumber = "Q1",
            BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion
        };
        var allQuestions = new ObjectListBase<Question> { question };

        // Act
        var result = QuestionEngine.GetQuestionsInBlock(question, allQuestions);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetQuestionsInBlock_ReturnsFullBlock()
    {
        // Arrange
        var q1 = new Question { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock };
        var q2 = new Question { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion };
        var q3 = new Question { QstNumber = "Q3", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion };
        var q4 = new Question { QstNumber = "Q4", BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion };

        var allQuestions = new ObjectListBase<Question> { q1, q2, q3, q4 };

        // Act
        var result = QuestionEngine.GetQuestionsInBlock(q2, allQuestions);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(q1);
        result.Should().Contain(q2);
        result.Should().Contain(q3);
        result.Should().NotContain(q4);
    }

    [Fact]
    public void GetQuestionsInBlock_FromFirstQuestion_ReturnsBlock()
    {
        // Arrange
        var q1 = new Question { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock };
        var q2 = new Question { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion };
        var q3 = new Question { QstNumber = "Q3", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion };

        var allQuestions = new ObjectListBase<Question> { q1, q2, q3 };

        // Act
        var result = QuestionEngine.GetQuestionsInBlock(q1, allQuestions);

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be(q1);
        result[2].Should().Be(q3);
    }

    [Fact]
    public void GetQuestionsInBlock_FromLastQuestion_ReturnsBlock()
    {
        // Arrange
        var q1 = new Question { QstNumber = "Q1", BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock };
        var q2 = new Question { QstNumber = "Q2", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion };
        var q3 = new Question { QstNumber = "Q3", BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion };

        var allQuestions = new ObjectListBase<Question> { q1, q2, q3 };

        // Act
        var result = QuestionEngine.GetQuestionsInBlock(q3, allQuestions);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(q1);
        result.Should().Contain(q2);
        result.Should().Contain(q3);
    }

    [Fact]
    public void IsQuestionFilled_ReturnsTrueWhenDataFileColAndNumberPresent()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = 1,
            QstNumber = "Q1"
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsQuestionFilled_ReturnsTrueWhenDataFileColAndLabelPresent()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = 1,
            QstLabel = "Question Label"
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsQuestionFilled_ReturnsFalseWhenDataFileColZero()
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
    public void IsQuestionFilled_ReturnsFalseWhenNoNumberOrLabel()
    {
        // Arrange
        var question = new Question
        {
            DataFileCol = 1,
            QstNumber = "",
            QstLabel = ""
        };

        // Act
        var result = QuestionEngine.IsQuestionFilled(question);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsQuestionFilled_ThrowsWhenNull()
    {
        // Act & Assert
        var act = () => QuestionEngine.IsQuestionFilled(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DefaultResponseIndex_UpdatesNoneToNeutral()
    {
        // Arrange
        var q1 = new Question
        {
            QstNumber = "Q1",
            BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock,
            Responses = new ObjectListBase<Response>
            {
                new(1, "Yes", ResponseIndexType.None),
                new(2, "No", ResponseIndexType.Positive)
            }
        };
        var q2 = new Question
        {
            QstNumber = "Q2",
            BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion,
            Responses = new ObjectListBase<Response>
            {
                new(1, "Yes", ResponseIndexType.None)
            }
        };
        var allQuestions = new ObjectListBase<Question> { q1, q2 };

        // Act
        QuestionEngine.DefaultResponseIndex(q1, allQuestions);

        // Assert
        q1.Responses[0].IndexType.Should().Be(ResponseIndexType.Neutral);
        q1.Responses[1].IndexType.Should().Be(ResponseIndexType.Positive); // Unchanged
        q2.Responses[0].IndexType.Should().Be(ResponseIndexType.Neutral);
    }

    [Fact]
    public void DefaultResponseIndex_HandlesDiscreetQuestion()
    {
        // Arrange
        var question = new Question
        {
            QstNumber = "Q1",
            BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion,
            Responses = new ObjectListBase<Response>
            {
                new(1, "Yes", ResponseIndexType.None)
            }
        };
        var allQuestions = new ObjectListBase<Question> { question };

        // Act
        QuestionEngine.DefaultResponseIndex(question, allQuestions);

        // Assert
        question.Responses[0].IndexType.Should().Be(ResponseIndexType.None); // Unchanged
    }

    [Fact]
    public void IsIVInSameBlockAsDV_ReturnsTrueWhenInSameBlock()
    {
        // Arrange
        var dvQuestion = new Question
        {
            Id = Guid.NewGuid(),
            QstNumber = "Q1",
            BlkQstStatus = BlkQuestionStatusType.FirstQuestionInBlock
        };
        var ivQuestion = new Question
        {
            Id = Guid.NewGuid(),
            QstNumber = "Q2",
            BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion
        };
        var allQuestions = new ObjectListBase<Question> { dvQuestion, ivQuestion };

        // Act
        var result = QuestionEngine.IsIVInSameBlockAsDV(dvQuestion, ivQuestion, allQuestions);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsIVInSameBlockAsDV_ReturnsFalseWhenInDifferentBlock()
    {
        // Arrange
        var dvQuestion = new Question
        {
            Id = Guid.NewGuid(),
            QstNumber = "Q1",
            BlkQstStatus = BlkQuestionStatusType.DiscreetQuestion
        };
        var ivQuestion = new Question
        {
            Id = Guid.NewGuid(),
            QstNumber = "Q2",
            BlkQstStatus = BlkQuestionStatusType.ContinuationQuestion
        };
        var allQuestions = new ObjectListBase<Question> { dvQuestion, ivQuestion };

        // Act
        var result = QuestionEngine.IsIVInSameBlockAsDV(dvQuestion, ivQuestion, allQuestions);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SetQuestionModified_UpdatesModifiedFieldsForDirtyQuestions()
    {
        // Arrange
        var q1 = new Question { QstNumber = "Q1" };
        q1.QstLabel = "Modified"; // Makes it dirty
        
        var q2 = new Question { QstNumber = "Q2" };
        q2.MarkClean(); // Not dirty

        var questions = new List<Question> { q1, q2 };

        // Act
        QuestionEngine.SetQuestionModified(questions);

        // Assert
        q1.ModifiedBy.Should().NotBeEmpty();
        q1.ModifiedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        q2.ModifiedBy.Should().BeEmpty();
    }

    [Fact]
    public void SetQuestionModified_UpdatesModifiedFieldsForDirtyResponses()
    {
        // Arrange
        var question = new Question
        {
            QstNumber = "Q1",
            Responses = new ObjectListBase<Response>
            {
                new(1, "Yes", ResponseIndexType.Positive)
            }
        };
        question.MarkClean();
        question.Responses[0].Label = "Modified"; // Makes response dirty
        question.QstLabel = "Also modified"; // Makes question dirty

        var questions = new List<Question> { question };

        // Act
        QuestionEngine.SetQuestionModified(questions);

        // Assert
        question.ModifiedBy.Should().NotBeEmpty();
        question.Responses[0].ModifiedBy.Should().NotBeEmpty();
        question.Responses[0].ModifiedOn.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
    }
}
