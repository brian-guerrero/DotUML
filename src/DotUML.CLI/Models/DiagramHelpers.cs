namespace DotUML.CLI.Models;

// Utility method to get visibility character
public static class DiagramHelpers
{
    public static char GetVisibilityCharacter(string visibility) => visibility switch
    {
        var v when v.Contains("public") => '+',
        var v when v.Contains("protected") => '#',
        var v when v.Contains("private") => '-',
        _ => '?'
    };
}
