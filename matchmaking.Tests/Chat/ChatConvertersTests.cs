using System.IO;
using Microsoft.UI.Xaml;

namespace matchmaking.Tests;

public sealed class ChatConvertersTests
{
    [Fact]
    public void ChatNameConverter_null_returns_empty_string()
    {
        var converter = new ChatNameConverter();

        var result = converter.Convert(null, typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void ReadReceiptConverter_true_false_and_null()
    {
        var converter = new ReadReceiptConverter();

        converter.Convert(true, typeof(string), null, string.Empty).Should().Be("Seen");
        converter.Convert(false, typeof(string), null, string.Empty).Should().Be("Delivered");
        converter.Convert(null, typeof(string), null, string.Empty).Should().Be("Delivered");
    }

    [Fact]
    public void MessageTextVisibilityConverter_handles_text_and_non_text()
    {
        var converter = new MessageTextVisibilityConverter();

        var text = new Message { Type = MessageType.Text };
        var image = new Message { Type = MessageType.Image };

        converter.Convert(text, typeof(Visibility), null, string.Empty).Should().Be(Visibility.Visible);
        converter.Convert(image, typeof(Visibility), null, string.Empty).Should().Be(Visibility.Collapsed);
        converter.Convert(null, typeof(Visibility), null, string.Empty).Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void MessageContentDisplayConverter_handles_text_and_attachment()
    {
        var converter = new MessageContentDisplayConverter();

        var text = new Message { Type = MessageType.Text, Content = "Hello" };
        var filePath = Path.Combine("C:\\", "tmp", "report.pdf");
        var file = new Message { Type = MessageType.File, Content = filePath };

        converter.Convert(text, typeof(string), null, string.Empty).Should().Be("Hello");
        converter.Convert(file, typeof(string), null, string.Empty).Should().Be("📎 report.pdf");
    }
}
