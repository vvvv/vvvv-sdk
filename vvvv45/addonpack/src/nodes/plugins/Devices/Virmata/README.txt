Welcome to the Firmata Plugin for VVVV
**************************************

A straight forward implementation of the Firmata protocol for VVVV. See http://firmata.org for details on the protocol. This implementation is designed from the protocol, rather than from the Arduino board perspective. This enables it to easily interface with a Firmata prepared Arduino board with a StandardFirmata, but is not bound to this (e.g. other boards, other Firmata). Thus it can be extened with additional modules, rather than plugins to use other Firmata enabled hardware.

This is still beta, but stable as far as we could test. Don't fear to report any issues or feature request to [the issues here on github](https://github.com/jens-a-e/VirmataEncoder/issues).


Installation & Use
******************

To use it with a standard Arduino, as this is the prefered use case, do the following:

1) Upload the StandardFirmata (version 2.2) that somes with the Arduino IDE onto your board.
1.a) If you have a freshly purchased and never touched Arduino UNO board you mustlikely already have the standard Firmata uploaded already.
2) Prepare some LED and other things to use and test with it.

3) Download the released ZIP (e.g. https://github.com/jens-a-e/VirmataEncoder/zipball/beta1) and put it into your vvvv *plugins* folder of your vvvv installation

4) Open the _Arduino (StandardFirmata 2.2) help_ patch in the modules folder (the HELP!)

4.a) Make sure you follow the instructions there and have your board connected to the right comport!

5) To use it in your own patches, copy the Arduino node there and adujst as needed.



The use of the Firmata nodes
****************************

If you look inside the Arduino module you will discover the basic usage of the Firmata nodes. As the data we pass arround is encoded into an ANSI string you can also pass it arround the network easly (examples to come) through the *FirmataEncode* and save bandwidth compared to a _normal_ string usage. You should read the [wiki page on the design issues on firmata.org](http://firmata.org/wiki/Design_Issues9), if you want to know, why this is a nice protocol in terms of bandwidth.

We offer the follwing nodes:

* FirmataEncode - well encodes firmata and handles the setup of pins, etc.
* FirmataDecode - aha, yes: decodes firmata messages to a fixed number of analog and digital values as two seperate spreads (you can set the sizes in the inspector)
* I2CDecode - it basicall just puts the registers and the related data from Firmata into 16bit values. This **does not** decode any specific data from a specific sensor


Still missing nodes:

* I2CEncode - to setup I2C and send data to I2C devices connected to the Firmata device


As we hope this approach makes the use of firmata less stressful.
