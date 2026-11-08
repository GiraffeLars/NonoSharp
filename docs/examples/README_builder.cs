using NonoSharp;

NonogramBuilder builder = NonogramBuilder.FromString(
    " O O \n" +
    "     \n" +
    "     \n" +
    "O   O\n" +
    "OOOOO",
    "Smiley");

if (builder.IsSolvable())
{
    Nonogram puzzle = builder.ToNonogram();
    puzzle.SaveAsFile("smiley.ns");
}
