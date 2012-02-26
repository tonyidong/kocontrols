How to set NUnit to execute the tests from within VS 2010
1. Install NUnit 2.5.10 from here: http://launchpad.net/nunitv2/2.5/2.5.10/+download/NUnit-2.5.10.11092.msi
2. Right click on the KOControls.GUI.Tests project and choose properties. In the properties windows select "Start external program:" then choose the full path to nunit.exe something like: C:\Program Files\NUnit 2.5.10\bin\net-2.0\nunit.exe
   For "Command line arguments:" type KOControls.GUI.Tests.dll.
   For "Working directory:" select the output directory of your KOControls.GUI.Tests project, something like: D:\KOControls\Software\Solutions\KOControls.GUI.Tests\bin\Debug\
3. You have to do one additoinal step to enable debugging in the unit tests right from VS. Open the nunit.exe.config file which should be by default here:  C:\Program Files\NUnit 2.5.10\bin\net-2.0\nunit.exe.config.
   Paste under <configuration> the following lines:
	 <startup>
		<requiredRuntime version="v4.0.30319" />
    </startup>

	Also under <runtime> paste the following:
		<loadFromRemoteSources enabled="true" />
Or for newer NUnit editions comment out the line:
	<supportedRuntime version="v2.0.50727" />-->
  
