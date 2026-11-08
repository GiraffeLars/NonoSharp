using NonoSharp;
NonogramBuilder builder = new(5, 5);

// Construct custom solution
builder.FillCell(1, 0); builder.FillCell(3, 0);

builder.FillCell(1, 1); builder.FillCell(3, 1);

builder.FillCell(0, 3); builder.FillCell(4, 3);

builder.FillCell(0, 4); builder.FillCell(1, 4); builder.FillCell(2, 4);
builder.FillCell(3, 4); builder.FillCell(4, 4);

// Save the constructed solution. Do not forget to catch the exceptions!
builder.SaveAsFile("builder_example.ns");
