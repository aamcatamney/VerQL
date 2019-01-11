using System;
using Xunit;
using VerQL.Core.Utils;

namespace VerQL.CoreTest
{
  public class ExtensionsTests
  {
    [Theory]
    [InlineData("my data")]
    [InlineData("[my [data")]
    [InlineData("[my] [data]")]
    [InlineData("[my data]")]
    [InlineData("my] data]")]
    public void RemoveSquareBrackets(string text)
    {
      var result = text.RemoveSquareBrackets();
      Assert.NotNull(result);
      Assert.NotEmpty(result);
      Assert.Equal<string>("my data", result);
    }

    [Theory]
    [InlineData("my data")]
    [InlineData("(my (data")]
    [InlineData("(my) (data)")]
    [InlineData("(my data)")]
    [InlineData("my) data)")]
    public void RemoveBrackets(string text)
    {
      var result = text.RemoveBrackets();
      Assert.NotNull(result);
      Assert.NotEmpty(result);
      Assert.Equal<string>("my data", result);
    }

    [Theory]
    [InlineData("[id] INT IDENTITY (1,1) NOT NULL, [location] NVARCHAR(MAX) DEFAULT ('Europe,London') NOT NULL")]
    public void TrueSplit(string text)
    {
      var result = text.TrueSplit(new[] { ',' });
      Assert.NotNull(result);
      Assert.Equal<int>(2, result.Count);
      Assert.Equal<string>("[id] INT IDENTITY (1,1) NOT NULL", result[0]);
      Assert.Equal<string>("[location] NVARCHAR(MAX) DEFAULT ('Europe,London') NOT NULL", result[1]);
    }
  }
}
