// create a css selector by replacing -hover with :hover
var MakeSelectorFilter = function(input)
{
  var input = input.rawString();
  return input.replace("-hover",":hover");
};
MakeSelectorFilter.filterName = "makecssselector";
MakeSelectorFilter.isSafe = false;
Library.addFilter("MakeSelectorFilter");


// some global variables to store width and height in
width=1;
height=1;

// set the global width variable {{value|setWidth}}
var SetWidth = function(input)
{
    width=input;
    return "";
};
SetWidth.filterName = "setWidth";
SetWidth.isSafe = false;
Library.addFilter("SetWidth");

// set the global height variable {{value|setHeight}}
var SetHeight = function(input)
{
    height=input;
    return "";
};
SetHeight.filterName = "setHeight";
SetHeight.isSafe = false;
Library.addFilter("SetHeight");

// calculate relative x value {{value|makeRelX}}
var MakeRelX = function(input)
{
    return String(input/width);
};
MakeRelX.filterName = "makeRelX";
MakeRelX.isSafe = false;
Library.addFilter("MakeRelX");

// calculate relative y value {{value|makeRelY}}
var MakeRelY = function(input)
{
    return String(input/height);
};
MakeRelY.filterName = "makeRelY";
MakeRelY.isSafe = false;
Library.addFilter("MakeRelY");
