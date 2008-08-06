Managed Library for Nintendo's Wiimote
v1.1.0.0
by Brian Peek (http://www.brianpeek.com/)

For more information, please visit the associated article for this project at:

http://msdn.microsoft.com/coding4fun/hardware/article.aspx?articleid=1879033

There you will find documentation on how all of this works.

If all else fails, please contact me at the address above.  Enjoy!

Changes
=======

v1.1.0.0
--------
	o Support for XP and Vista x64 (Paul Miller)
	o VB fix in ParseExtension (Evan Merz)
	o New "AltWriteMethod" property which will try a secondary approach to writing
	  to the Wiimote.  If you get an error when connecting, set this property and
	  try again to see if it fixes the issue.
	o Microsoft Robotics Studio project
	  Open the WiimoteMSRS directory and start the Wiimote.sln solution to take a
	  look! (David Lee)

v1.0.1.0
--------
	o Calibration copy/paste error (James Darpinian)