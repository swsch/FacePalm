# FacePalm
Face/Palm Measuring Tool: A stand-alone software for standardized hand and face morphometry

This tool helps researchers to manually pinpoint the locations required to perform standardized hand and face morphometry.

Please cite this software as: 
Köllner, M. G., Schmiedl, H., Waßer, J., & Schmiedl, S. (2017). Face/Palm Measuring Tool: A stand-alone software for standardized hand and face morphometry [Software and manual]. Erlangen. Retrieved from http://www.psych2.phil.uni-erlangen.de/~oschult/humanlab/resources/resources_Facepalm.htm


## Elevator Pitch
The _research organizer_ defines lists of landmarks, which in turn define lines and measurable segments in a simple CSV file to describe the data set required for a given investigation.
_Research assistants_ then manually locate the required points on photographs or scans, lines and segments are drawn automatically once their defining points have been set.
Finally point coordinates and segment lengths are exported to a CSV file for further processing, while graphical elements can be hidden or shown to prepare visual material for presentations.

### Download Binary Stable Release
A precompiled binary release archive for modern Windows systems 
(Win 7 or later with .NET 4.5) of the software and manual along with the 
description files („face.csv“, „palm.csv“) will shortly be available from [FacePalm_v1.0](http://www.psych2.phil.uni-erlangen.de/~oschult/humanlab/resources/resources_Facepalm.htm).

### Build Instructions
FacePalm is being developed with Microsoft Visual Studio 2017 Enterprise, 
but should build with other editions as long as it can make sense of the
solution file.

## How to Prepare a Definitions File

Definition files contain descriptions of landmarks and derived geometric objects like line segments
of finite length and straight lines. The items listed in a definitions file build a To-Do-List for
measuring the individual images.

### File Structure

A definitions file is a basic CSV file (currently using `;` as field separator). A line in this file is
considered to be usable data if it starts with one of the following key words:

  * `point`
  * `segment`
  * `line`

All other lines are not used by FacePalm and can be freely used to provide comments or history of modifications.

The example files provided with FacePalm use this to provide "column headings" for the three sections:

```
TYPE;NAME OF LINE;NAME OF FIRST POINT;NAME OF SECOND POINT;DESCRIPTION
line;A;2;7;middle line (ML)
...
;;;;
TYPE;NAME OF SEGMENT;NAME OF FIRST POINT;NAME OF SECOND POINT;DESCRIPTION
segment;d1;23;24;outside eye corners
...
;;;;
TYPE;GROUP;NAME OF POINT;DESCRIPTION;
point;eyes;23;outside eye corner, left;
...
```

The "names" within each section must be unique to be useful. The items are used in the same order as they are listed in the definitions, both on screen and during data export.
This gives the research organizer the maximum amount of control.

### Defining Points

Points require the following fields in exactly this sequence:

  1. the keyword `point`
  2. group name: this is currently not used by FacePalm, but intended to help manage large lists of points
  3. name of the point: must be unique to identify points for lines and segments
  4. informative description of the location

Points use their number as part of coordinate labels during export: The coordinates for point `23` are written to columns `X23` and `Y23`.

### Defining Line Segments

Line segments require the following fields in exactly this sequence:

  1. the keyword `segment`
  2. name of the segment: a text label
  3. name of the first point defining the segment
  4. name of the second point defining the segment
  5. description of the line segment

Line segments use their name as column header during export: The length of line `d1` shows up in column `d1`.
The segment name is displayed below the midpoint of the line segment drawn on the image.

### Defining Lines

Lines require the following fields in exactly this sequence:

  1. the keyword `line`
  2. name of the line: a text label
  3. name of the first point defining the line
  4. name of the second point defining the line
  5. description of the line segment

Lines are currently not exported.
The line name is displayed near the first point outside of the segment defined by the two points.

## How to measure 2d images

todo
