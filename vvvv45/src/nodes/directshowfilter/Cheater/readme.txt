delphi5 code for cheater directshow filter.

the main purpose of this filter is to convert mediasamples of type VideoInfoHeader to type VideoInfoHeader2 so that they can be deinterlaced via VMR9.


via the ICheaterParameters interface you can set some additional options:
- FirstFieldFirst: decide which field goes first (switch between pal/ntsc)
- ClearSampleTimes: clear timestamps of the samples 
- DropSample: yeah.
- GetTimes: read reference and streamtime of sample

to compile you'll need the ds-baseclasses from progdigy:
http://www.progdigy.com/modules.php?name=DSPack