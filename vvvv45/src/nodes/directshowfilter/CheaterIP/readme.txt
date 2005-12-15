delphi5 code for cheaterip directshow filter.

the main purpose of this tranform-inplace-filter is to set the firstfield property of  mediasamples of type VideoInfoHeader2 to the desired value so that VMR9 deinterlaces correctly. useful e.g. in connection with http://btwincap.sourceforge.net/ driver that outputs VIH2 samples but doesn't let you set the correct field order.


via the ICheaterParameters interface you can set some additional options:
- FirstFieldFirst: decide which field goes first (switch between pal/ntsc)
- ClearSampleTimes: clear timestamps of the samples 
- DropSample: yeah.
- GetTimes: read reference and streamtime of sample

to compile you'll need the ds-baseclasses from progdigy:
http://www.progdigy.com/modules.php?name=DSPack


