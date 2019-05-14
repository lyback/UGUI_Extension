var MakeSelectorFilter = function(input)
{
  var input = input.rawString();
  return input.replace("-hover",":hover");
};
MakeSelectorFilter.filterName = "makecssselector";
MakeSelectorFilter.isSafe = false;
Library.addFilter("MakeSelectorFilter");