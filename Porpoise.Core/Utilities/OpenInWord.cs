// Porpoise.Core/Utilities/OpenInWord.cs
public class OpenInWord
{
    private readonly string _rtfPath;
    public OpenInWord(string rtfPath) => _rtfPath = rtfPath;

    public void OpenRTFInWord()
    {
        var word = new Microsoft.Office.Interop.Word.Application();
        word.Visible = true;
        word.Documents.Open(_rtfPath);
    }
}