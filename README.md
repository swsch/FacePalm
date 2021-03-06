# FacePalm
Face/Palm Measuring Tool: A stand-alone software for standardized hand and face morphometry

This tool helps researchers to manually pinpoint the locations required to perform standardized hand and face morphometry.

Please cite this software (current release 1.0.7) as: 
> K�llner, M. G., Schmiedl, H., Wa�er, J., & Schmiedl, S. (2017). Face/Palm Measuring Tool: A stand-alone software for standardized hand and face morphometry [Software and manual]. Erlangen. 
> Retrieved from https://github.com/swsch/FacePalm/releases/download/rel_1.0.7/FacePalm_1.0.7.zip


## Summary
The _researcher_ first defines lists of measurement points file relevant to her or his study in a simple CSV file.
These points in turn can be used to define lines and measurable segments (distances between points) within the same CSV file to describe the data set required for a given investigation.

The _researcher�s team_ then manually locate the required points on photographs or scans.
Lines and segments are drawn automatically once their defining points have been set.

Finally point coordinates and segment lengths are exported to a CSV file for further processing, while graphical elements can be hidden or shown to prepare visual material for presentations.

## Download Binary Stable Release
A precompiled binary release archive for modern Windows systems 
(Win 7 or later with .NET 4.5) of the software and manual along with the 
description files (�face.csv�, �palm.csv�) is currently available [here on github](https://github.com/swsch/FacePalm/releases/latest).

## Building from Source
FacePalm is being developed with Microsoft Visual Studio 2017 Enterprise, 
but should build with other tool like Jetbrains Rider as long as it can make sense of the
solution file.

## Documentation

  * [How to Prepare a Definitions File](https://github.com/swsch/FacePalm/wiki/HowTo_Definitions)
  * [How to Measure 2d Images](https://github.com/swsch/FacePalm/wiki/HowTo_Measure)
