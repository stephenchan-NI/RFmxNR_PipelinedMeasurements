//Steps:
//1. Open a new RFmx Session.
//2. Configure Frequency Reference.
//3. Configure Selected Ports.
//4. Configure basic signal properties (Center Frequency, Reference Level and External Attenuation).
//5. Configure Trigger Type and Trigger Parameters.
//6. Configure Link Direction as Uplink, Frequency Range, Band, Component Carrier and Subcarrier Spacing.
//7. Select ModAcc, ACP, CHP, OBW and SEM measurements and enable Traces.
//8. Configure ACP Sweep Time.
//9. Configure CHP Sweep Time.
//10. Configure OBW Sweep Time.
//11. Configure SEM Sweep Time.
//12. Configure Averaging Parameters for ModAcc.
//13. Configure Averaging Parameters for ACP.
//14. Configure Averaging Parameters for CHP.
//15. Configure Averaging Parameters for OBW.
//16. Configure Averaging Parameters for SEM.
//17. Configure Measurement Interval.
//18. Configure Uplink Mask Type, or Downlink Mask, gNodeB Category, Delta F_Max (Hz) and
//    Component Carrier Rated Output Power depending on Link Direction.
//19. Initiate the Measurement.
//20. Fetch ModAcc Measurements.
//21. Fetch ACP Measurements.
//22. Fetch CHP Measurements.
//23. Fetch OBW Measurements.
//24. Fetch SEM Measurements.
//25. Close RFmx Session.

using System;
using System.Collections.Generic;
using System.Linq;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.RFmx.NRMX;

namespace NationalInstruments.Examples.RFmxNRModAccAcpChpObwSemPiplinedSingleCarrier
{
   public class RFmxNRModAccAcpChpObwSemPipelinedSingleCarrier
   {
        RFmxInstrMX instrSession;
        RFmxNRMX NR_offsets;
        RFmxNRMX NR_carrier;
        string resourceName;

        string selectedPorts;
        double centerFrequency;
        double referenceLevel;
        double externalAttenuation;

        string frequencyReferenceSource;
        double frequencyReferenceFrequency;

        bool enableTrigger;
        string digitalEdgeSource;
        RFmxNRMXDigitalEdgeTriggerEdge digitalEdge;
        double triggerDelay;

        RFmxNRMXLinkDirection linkDirection;
        RFmxNRMXFrequencyRange frequencyRange;
        double carrierBandwidth;
        double subcarrierSpacing;
        int modaccBand;

        RFmxNRMXModAccMeasurementLengthUnit measurementLengthUnit;
        double measurementOffset;
        double measurementLength;

        RFmxNRMXSemUplinkMaskType uplinkMaskType;

        RFmxNRMXgNodeBCategory gNodeBCategory;
        RFmxNRMXSemDownlinkMaskType downlinkMaskType;
        double deltaFMaximum;
        double componentCarrierRatedOutputPower;

        double sweepTimeInterval;
        int averagingCount;

        double timeout;

        double compositeRmsEvmMean;                                                         /* (%) */
        double compositePeakEvmMaximum;                                                     /* (%) */
        double componentCarrierFrequencyErrorMean;                                          /* (Hz) */
        double componentCarrierIQOriginOffsetMean;                                          /* (dBc) */

        double chpAbsolutePower;                                                            /* (dBm) */
        double chpRelativePower;                                                            /* (dB) */

        double acpAbsolutePower;                                                            /* (dBm) */
        double acpRelativePower;                                                            /* (dB) */
        double[] acpLowerRelativePower;                                                     /* (dB) */
        double[] acpUpperRelativePower;                                                     /* (dB) */
        double[] acpLowerAbsolutePower;                                                     /* (dBm) */
        double[] acpUpperAbsolutePower;                                                     /* (dBm) */

        double obwOccupiedBandwidth;                                                        /* (Hz) */
        double obwAbsolutePower;                                                            /* (dBm) */
        double obwStartFrequency;                                                           /* (Hz) */
        double obwStopFrequency;                                                            /* (Hz) */

        RFmxNRMXSemMeasurementStatus semMeasurementStatus;
        double semAbsoluteIntegratedPower;                                                  /* (dBm) */
        double semRelativeIntegratedPower;                                                  /* (dB) */
        double semPeakAbsoluteIntegratedPower;
        double semPeakFrequency;
        RFmxNRMXSemLowerOffsetMeasurementStatus[] semLowerOffsetMeasurementStatus;
        double[] semLowerOffsetMargin;                                                      /* (dB) */
        double[] semLowerOffsetMarginFrequency;                                             /* (Hz) */
        double[] semLowerOffsetMarginAbsolutePower;                                         /* (dBm) */
        double[] semLowerOffsetMarginRelativePower;                                         /* (dB) */
        RFmxNRMXSemUpperOffsetMeasurementStatus[] semUpperOffsetMeasurementStatus;
        double[] semUpperOffsetMargin;                                                      /* (dB) */
        double[] semUpperOffsetMarginFrequency;                                             /* (Hz) */
        double[] semUpperOffsetMarginAbsolutePower;                                         /* (dBm) */
        double[] semUpperOffsetMarginRelativePower;                                         /* (dB) */
        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        double[] executionTimes; 
      public void Run()
      {
            try
            {
                InitializeVariables();
                InitializeInstr();
                List<double> tempTimes = new List<double>();
                for (int i = 0; i < 100; i++)
                {
                    ConfigureNR();
                    tempTimes.Add(RetrieveResults());
                }
                executionTimes = tempTimes.ToArray();
                var sum = executionTimes.Sum();
                var average = sum / executionTimes.Length;
                Console.WriteLine($"Execution Time: {average} ms");
                PrintResults();
            }
               catch (Exception ex)
            {
                DisplayError(ex);
            }
                finally
            {
            /* Close session */
                CloseSession();
                Console.WriteLine("\nPress any key to exit");
                Console.ReadKey();
            }
      } 

      private void InitializeVariables()
      {
            resourceName = "BCN_02";

            frequencyReferenceSource = RFmxInstrMXConstants.OnboardClock;
            frequencyReferenceFrequency = 10.0e6;                                            /* (Hz) */

            selectedPorts = "if0";
            centerFrequency = 9e9;                                                         /* (Hz) */
            referenceLevel = 0.00;                                                           /* (dBm) */
            externalAttenuation = 0.0;                                                       /* (dB) */

            enableTrigger = true;
            digitalEdgeSource = RFmxNRMXConstants.PxiTriggerLine0;
            digitalEdge = RFmxNRMXDigitalEdgeTriggerEdge.Rising;
            triggerDelay = 0.0;                                                              /* (s) */

            linkDirection = RFmxNRMXLinkDirection.Uplink;
            frequencyRange = RFmxNRMXFrequencyRange.Range2;
            carrierBandwidth = 100e6;                                                        /* (Hz) */
            subcarrierSpacing = 60e3;                                                        /* (Hz) */
            modaccBand = 78;

            measurementLengthUnit = RFmxNRMXModAccMeasurementLengthUnit.Slot;
            measurementOffset = 0.0;
            measurementLength = 1;

            uplinkMaskType = RFmxNRMXSemUplinkMaskType.General;

            gNodeBCategory = RFmxNRMXgNodeBCategory.WideAreaBaseStationCategoryA;
            downlinkMaskType = RFmxNRMXSemDownlinkMaskType.Standard;
            deltaFMaximum = 15.0e6;                                                          /* (Hz) */
            componentCarrierRatedOutputPower = 0.0;                                          /* (dBm) */

            sweepTimeInterval = 1.0e-3;                                                      /* (s) */

            averagingCount = 10;

            timeout = 10.0;                                                                  /* (s) */
      }

      private void InitializeInstr()
      {
            /* Create a new RFmx Session */
            instrSession = new RFmxInstrMX(resourceName, "");
      }

      private void ConfigureNR()
      {
            watch.Start();

            NR_offsets = instrSession.GetNRSignalConfiguration("signal::offsets");
            NR_carrier = instrSession.GetNRSignalConfiguration("signal::carrier");
        
            instrSession.ConfigureFrequencyReference("", frequencyReferenceSource, frequencyReferenceFrequency);
//            instrSession.SetDownconverterFrequencyOffset("signal::offsets", 256e6);

            //Configure offset measurements
            NR_offsets.SetSelectedPorts("", selectedPorts);
            NR_offsets.ConfigureRF("", centerFrequency, referenceLevel, externalAttenuation);
            NR_offsets.ConfigureDigitalEdgeTrigger("", digitalEdgeSource, digitalEdge, triggerDelay, enableTrigger);
            NR_offsets.SetLinkDirection("", linkDirection);
            NR_offsets.SetFrequencyRange("", frequencyRange);
            NR_offsets.SetBand("", modaccBand);
            NR_offsets.ComponentCarrier.SetBandwidth("", carrierBandwidth);
            NR_offsets.ComponentCarrier.SetBandwidthPartSubcarrierSpacing("", subcarrierSpacing);
            NR_offsets.SelectMeasurements("", RFmxNRMXMeasurementTypes.Acp | RFmxNRMXMeasurementTypes.Sem, true);


            //Configure carrier measurements
            NR_carrier.SetSelectedPorts("", selectedPorts);
            NR_carrier.ConfigureRF("", centerFrequency, referenceLevel, externalAttenuation);
            NR_carrier.ConfigureDigitalEdgeTrigger("", digitalEdgeSource, digitalEdge, triggerDelay, enableTrigger);
            NR_carrier.SetLinkDirection("", linkDirection);
            NR_carrier.SetFrequencyRange("", frequencyRange);
            NR_carrier.SetBand("", modaccBand);
            NR_carrier.ComponentCarrier.SetBandwidth("", carrierBandwidth);
            NR_carrier.ComponentCarrier.SetBandwidthPartSubcarrierSpacing("", subcarrierSpacing);
            NR_carrier.SelectMeasurements("", RFmxNRMXMeasurementTypes.ModAcc | RFmxNRMXMeasurementTypes.Chp | RFmxNRMXMeasurementTypes.Obw, true);

            NR_offsets.Acp.Configuration.ConfigureSweepTime("", RFmxNRMXAcpSweepTimeAuto.True, sweepTimeInterval);

            NR_offsets.Chp.Configuration.ConfigureSweepTime("", RFmxNRMXChpSweepTimeAuto.True, sweepTimeInterval);

            NR_offsets.Obw.Configuration.ConfigureSweepTime("", RFmxNRMXObwSweepTimeAuto.True, sweepTimeInterval);

            NR_offsets.Sem.Configuration.ConfigureSweepTime("", RFmxNRMXSemSweepTimeAuto.True, sweepTimeInterval);

            NR_carrier.ModAcc.Configuration.SetAveragingEnabled("", RFmxNRMXModAccAveragingEnabled.False);
            NR_carrier.ModAcc.Configuration.SetAveragingCount("", averagingCount);

            NR_offsets.Acp.Configuration.ConfigureAveraging("", RFmxNRMXAcpAveragingEnabled.False, averagingCount,
            RFmxNRMXAcpAveragingType.Rms);

            NR_carrier.Chp.Configuration.ConfigureAveraging("", RFmxNRMXChpAveragingEnabled.False, averagingCount,
            RFmxNRMXChpAveragingType.Rms);

            NR_carrier.Obw.Configuration.ConfigureAveraging("", RFmxNRMXObwAveragingEnabled.False, averagingCount,
            RFmxNRMXObwAveragingType.Rms);

            NR_offsets.Sem.Configuration.ConfigureAveraging("", RFmxNRMXSemAveragingEnabled.False, averagingCount,
            RFmxNRMXSemAveragingType.Rms);

            NR_carrier.ModAcc.Configuration.SetMeasurementLengthUnit("", measurementLengthUnit);
            NR_carrier.ModAcc.Configuration.SetMeasurementOffset("", measurementOffset);
            NR_carrier.ModAcc.Configuration.SetMeasurementLength("", measurementLength);

            if(linkDirection == RFmxNRMXLinkDirection.Uplink)
                {
                    NR_offsets.Sem.Configuration.ConfigureUplinkMaskType("", uplinkMaskType);
                }
            else
                {
                    NR_offsets.ConfiguregNodeBCategory("", gNodeBCategory);
                    NR_offsets.Sem.Configuration.SetDownlinkMaskType("", downlinkMaskType);
                    NR_offsets.Sem.Configuration.SetDeltaFMaximum("", deltaFMaximum);
                    NR_offsets.Sem.Configuration.ComponentCarrier.ConfigureRatedOutputPower("", componentCarrierRatedOutputPower);
                }
            NR_offsets.Commit("");
            NR_carrier.Commit("");
            NR_carrier.Initiate("", "");
            NR_carrier.WaitForMeasurementComplete("", timeout); 
            NR_offsets.Initiate("", "");
            //NR_offsets.WaitForMeasurementComplete("", timeout);
//            NR_carrier.Initiate("", "");
      }

      private double RetrieveResults()
      {
            NR_carrier.ModAcc.Results.GetCompositeRmsEvmMean("", out compositeRmsEvmMean);
            NR_carrier.ModAcc.Results.GetCompositePeakEvmMaximum("", out compositePeakEvmMaximum);
            NR_carrier.ModAcc.Results.GetComponentCarrierFrequencyErrorMean("", out componentCarrierFrequencyErrorMean);
            NR_carrier.ModAcc.Results.GetComponentCarrierIQOriginOffsetMean("", out componentCarrierIQOriginOffsetMean);

             NR_offsets.Acp.Results.FetchOffsetMeasurementArray("", timeout, ref acpLowerRelativePower,
                ref acpUpperRelativePower, ref acpLowerAbsolutePower, ref acpUpperAbsolutePower);

             NR_offsets.Acp.Results.ComponentCarrier.FetchMeasurement("", timeout, out acpAbsolutePower, out acpRelativePower);

            NR_carrier.Chp.Results.ComponentCarrier.FetchMeasurement("", timeout, out chpAbsolutePower, out chpRelativePower);

            NR_carrier.Obw.Results.FetchMeasurement("", timeout, out obwOccupiedBandwidth, out obwAbsolutePower,
                out obwStartFrequency, out obwStopFrequency);

             NR_offsets.Sem.Results.FetchLowerOffsetMarginArray("", timeout, ref semLowerOffsetMeasurementStatus,
                ref semLowerOffsetMargin, ref semLowerOffsetMarginFrequency, ref semLowerOffsetMarginAbsolutePower,
                ref semLowerOffsetMarginRelativePower);

             NR_offsets.Sem.Results.FetchUpperOffsetMarginArray("", timeout, ref semUpperOffsetMeasurementStatus,
                ref semUpperOffsetMargin, ref semUpperOffsetMarginFrequency, ref semUpperOffsetMarginAbsolutePower,
                ref semUpperOffsetMarginRelativePower);

             NR_offsets.Sem.Results.ComponentCarrier.FetchMeasurement("", timeout, out semAbsoluteIntegratedPower,
                out semPeakAbsoluteIntegratedPower, out semPeakFrequency, out semRelativeIntegratedPower);

             NR_offsets.Sem.Results.FetchMeasurementStatus("", timeout, out semMeasurementStatus);
            watch.Stop();
            var timeElapsed = watch.ElapsedMilliseconds;
            watch.Reset();
            return timeElapsed;
        }

      private void PrintResults()
      {
             Console.WriteLine("************************* ModAcc *************************\n");
             Console.WriteLine("Composite RMS EVM Mean (%)                     : {0}", compositeRmsEvmMean);
             Console.WriteLine("Composite Peak EVM Maximum (% )                : {0}", compositePeakEvmMaximum);
             Console.WriteLine("Component Carrier Frequency Error Mean (Hz)    : {0}", componentCarrierFrequencyErrorMean);
             Console.WriteLine("Component Carrier IQ Origin Offset Mean (dBc)  : {0}\n", componentCarrierIQOriginOffsetMean);

             Console.WriteLine("\n\n************************* CHP *************************\n");
             Console.WriteLine("Carrier Absolute Power (dBm)                   : {0}\n", chpAbsolutePower);

             Console.WriteLine("\n\n************************* ACP *************************\n");
             Console.WriteLine("Carrier Absolute Power (dBm)                   : {0}", acpAbsolutePower);
             Console.WriteLine("\n------- Offset Channel Measurements -------");
             for (int i = 0; i < acpLowerRelativePower.Length; i++)
             {
                Console.WriteLine("\nOffset  {0}", i);
                Console.WriteLine("Lower Relative Power (dB)                      : {0}", acpLowerRelativePower[i]);
                Console.WriteLine("Upper Relative Power (dB)                      : {0}", acpUpperRelativePower[i]);
                Console.WriteLine("Lower Absolute Power (dBm)                     : {0}", acpLowerAbsolutePower[i]);
                Console.WriteLine("Upper Absolute Power (dBm)                     : {0}", acpUpperAbsolutePower[i]);
             }

             Console.WriteLine("\n\n\n************************* OBW *************************\n");
             Console.WriteLine("Occupied Bandwidth (Hz)                        : {0}", obwOccupiedBandwidth);
             Console.WriteLine("Absolute Power (dBm)                           : {0}", obwAbsolutePower);
             Console.WriteLine("Start Frequency (Hz)                           : {0}", obwStartFrequency);
             Console.WriteLine("Stop Frequency (Hz)                            : {0}\n", obwStopFrequency);

             Console.WriteLine("\n\n************************* SEM *************************\n");
             Console.WriteLine("Measurement Status                             : {0}", semMeasurementStatus);
             Console.WriteLine("Carrier Absolute Integrated Power (dBm)        : {0}", semAbsoluteIntegratedPower);
             Console.WriteLine("\n----- Lower Offset Segment Measurements -----");
             for (int i = 0; i < semLowerOffsetMargin.Length; i++)
             {
                Console.WriteLine("\nOffset  {0}", i);
                Console.WriteLine("Measurement Status                             : {0}", semLowerOffsetMeasurementStatus[i]);
                Console.WriteLine("Margin (dB)                                    : {0}", semLowerOffsetMargin[i]);
                Console.WriteLine("Margin Frequency (Hz)                          : {0}", semLowerOffsetMarginFrequency[i]);
                Console.WriteLine("Margin Absolute Power (dBm)                    : {0}", semLowerOffsetMarginAbsolutePower[i]);

             }
             Console.WriteLine("\n----- Upper Offset Segment Measurements -----");
             for (int i = 0; i < semUpperOffsetMargin.Length; i++)
             {
                Console.WriteLine("\nOffset  {0}", i);
                Console.WriteLine("Measurement Status                             : {0}", semUpperOffsetMeasurementStatus[i]);
                Console.WriteLine("Margin (dB)                                    : {0}", semUpperOffsetMargin[i]);
                Console.WriteLine("Margin Frequency (Hz)                          : {0}", semUpperOffsetMarginFrequency[i]);
                Console.WriteLine("Margin Absolute Power (dBm)                    : {0}", semUpperOffsetMarginAbsolutePower[i]);
             }
      }

      private void CloseSession()
      {
             try
             {
                if (NR_offsets != null)
                {
                   NR_offsets.Dispose();
                   NR_offsets = null;
                }
                if (NR_carrier != null)
                {
                    NR_carrier.Dispose();
                    NR_carrier = null;
                }
                if (instrSession != null)
                {
                   instrSession.Close();
                   instrSession = null;
                }
             }
             catch (Exception ex)
             {
                DisplayError(ex);
             }
      }

      static private void DisplayError(Exception ex)
      {
             Console.WriteLine("ERROR:\n" + ex.GetType() + ": " + ex.Message);
      }

   }
}
