using FluentAssertions;
using PetLoveCommunity.Application.DTOs.Shared;
using System.Text.Json;

namespace PetLoveCommunity.Tests.Unit.Application.DTOs.Shared;

public class ApiResponseTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResponse_WithNoMessage()
    {
        // Act
        var response = ApiResponse.Success();

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void Fail_ShouldCreateFailedResponse_WithMessage()
    {
        // Arrange
        const string errorMessage = "An error occurred";

        // Act
        var response = ApiResponse.Fail(errorMessage);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().Be(errorMessage);
    }

    [Fact]
    public void Fail_ShouldCreateFailedResponse_WithNullMessage()
    {
        // Act
        var response = ApiResponse.Fail(null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldSetProperties_Correctly()
    {
        // Arrange
        const bool isSuccess = true;
        const string message = "Test message";

        // Act
        var response = new ApiResponse(isSuccess, message);

        // Assert
        response.IsSuccess.Should().Be(isSuccess);
        response.Message.Should().Be(message);
    }

    [Fact]
    public void Record_ShouldBeImmutable()
    {
        // Arrange
        var response1 = ApiResponse.Success();
        var response2 = ApiResponse.Success();

        // Assert
        response1.Should().Be(response2);
        response1.GetHashCode().Should().Be(response2.GetHashCode());
    }

    [Fact]
    public void JsonSerialization_ShouldWork_Correctly()
    {
        // Arrange
        var response = ApiResponse.Fail("Test error");

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<ApiResponse>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.IsSuccess.Should().BeFalse();
        deserialized.Message.Should().Be("Test error");
    }
}

public class ApiResponseGenericTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResponse_WithData()
    {
        // Arrange
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        var response = ApiResponse<TestData>.Success(testData);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Be(testData);
        response.Message.Should().BeNull();
    }

    [Fact]
    public void Fail_ShouldCreateFailedResponse_WithMessage_AndDefaultData()
    {
        // Arrange
        const string errorMessage = "An error occurred";

        // Act
        var response = ApiResponse<TestData>.Fail(errorMessage);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be(errorMessage);
    }

    [Fact]
    public void Fail_ShouldCreateFailedResponse_WithNullMessage_AndDefaultData()
    {
        // Act
        var response = ApiResponse<TestData>.Fail(null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldSetProperties_Correctly()
    {
        // Arrange
        const bool isSuccess = true;
        var testData = new TestData { Id = 1, Name = "Test" };
        const string message = "Test message";

        // Act
        var response = new ApiResponse<TestData>(isSuccess, testData, message);

        // Assert
        response.IsSuccess.Should().Be(isSuccess);
        response.Data.Should().Be(testData);
        response.Message.Should().Be(message);
    }

    [Fact]
    public void Record_ShouldBeImmutable_WithSameData()
    {
        // Arrange
        var testData = new TestData { Id = 1, Name = "Test" };
        var response1 = ApiResponse<TestData>.Success(testData);
        var response2 = ApiResponse<TestData>.Success(testData);

        // Assert
        response1.Should().Be(response2);
        response1.GetHashCode().Should().Be(response2.GetHashCode());
    }

    [Fact]
    public void Record_ShouldNotBeEqual_WithDifferentData()
    {
        // Arrange
        var testData1 = new TestData { Id = 1, Name = "Test1" };
        var testData2 = new TestData { Id = 2, Name = "Test2" };
        var response1 = ApiResponse<TestData>.Success(testData1);
        var response2 = ApiResponse<TestData>.Success(testData2);

        // Assert
        response1.Should().NotBe(response2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Error message")]
    public void Success_WithNullData_ShouldWork(string? message)
    {
        // Act
        var response = new ApiResponse<TestData?>(true, null, message);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().BeNull();
        response.Message.Should().Be(message);
    }

    [Fact]
    public void JsonSerialization_ShouldWork_WithComplexData()
    {
        // Arrange
        var testData = new TestData { Id = 1, Name = "Test" };
        var response = ApiResponse<TestData>.Success(testData);

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<ApiResponse<TestData>>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.IsSuccess.Should().BeTrue();
        deserialized.Data.Should().NotBeNull();
        deserialized.Data!.Id.Should().Be(1);
        deserialized.Data.Name.Should().Be("Test");
        deserialized.Message.Should().BeNull();
    }

    [Fact]
    public void JsonSerialization_ShouldWork_WithArrayData()
    {
        // Arrange
        var testData = new[] 
        { 
            new TestData { Id = 1, Name = "Test1" },
            new TestData { Id = 2, Name = "Test2" }
        };
        var response = ApiResponse<TestData[]>.Success(testData);

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<ApiResponse<TestData[]>>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.IsSuccess.Should().BeTrue();
        deserialized.Data.Should().NotBeNull();
        deserialized.Data!.Should().HaveCount(2);
        deserialized.Data[0].Id.Should().Be(1);
        deserialized.Data[1].Id.Should().Be(2);
    }

    [Fact]
    public void JsonSerialization_ShouldWork_WithFailedResponse()
    {
        // Arrange
        var response = ApiResponse<TestData>.Fail("Test error");

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<ApiResponse<TestData>>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.IsSuccess.Should().BeFalse();
        deserialized.Data.Should().BeNull();
        deserialized.Message.Should().Be("Test error");
    }

    [Fact]
    public void Success_WithMessage_ShouldCreateSuccessfulResponse_WithDataAndMessage()
    {
        // Arrange
        var testData = new TestData { Id = 1, Name = "Test" };
        const string message = "Operation completed successfully";

        // Act
        var response = ApiResponse<TestData>.Success(testData, message);

        // Assert
        response.IsSuccess.Should().BeTrue();
        response.Data.Should().Be(testData);
        response.Message.Should().Be(message);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResponse_WithMessage()
    {
        // Arrange
        const string errorMessage = "Operation failed";

        // Act
        var response = ApiResponse<TestData>.Failure(errorMessage);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_WithNullMessage_ShouldCreateFailedResponse()
    {
        // Act
        var response = ApiResponse<TestData>.Failure(null);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().BeNull();
    }

    [Fact]
    public void ValidationError_WithErrors_ShouldCreateFailedResponse_WithFormattedMessage()
    {
        // Arrange
        var errors = new List<string> { "Email is required", "Password is too short", "Name cannot be empty" };

        // Act
        var response = ApiResponse<TestData>.ValidationError(errors);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be("Validation failed: Email is required, Password is too short, Name cannot be empty");
    }

    [Fact]
    public void ValidationError_WithSingleError_ShouldCreateFailedResponse_WithFormattedMessage()
    {
        // Arrange
        var errors = new List<string> { "Email is required" };

        // Act
        var response = ApiResponse<TestData>.ValidationError(errors);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be("Validation failed: Email is required");
    }

    [Fact]
    public void ValidationError_WithEmptyErrors_ShouldCreateFailedResponse_WithBaseMessage()
    {
        // Arrange
        var errors = new List<string>();

        // Act
        var response = ApiResponse<TestData>.ValidationError(errors);

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Data.Should().BeNull();
        response.Message.Should().Be("Validation failed: ");
    }

    [Fact]
    public void Fail_And_Failure_ShouldBehaveSimilarly()
    {
        // Arrange
        const string errorMessage = "Test error";

        // Act
        var failResponse = ApiResponse<TestData>.Fail(errorMessage);
        var failureResponse = ApiResponse<TestData>.Failure(errorMessage);

        // Assert
        failResponse.IsSuccess.Should().Be(failureResponse.IsSuccess);
        failResponse.Data.Should().Be(failureResponse.Data);
        failResponse.Message.Should().Be(failureResponse.Message);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}