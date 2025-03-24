Imports System

Module QuantizePlus
    ''' <summary>
    ''' Creates a frequency distribution mapping of the values in InputData. This can be called multiple times on
    ''' different InputData and the same InitialDictionary to enable chunking.
    ''' </summary>
    ''' <param name="InputData">FP16 input data as a byte array</param>
    ''' <param name="InitialDictionary">Initial Dictionary from previous call or a new 
    ''' DGroupArray initialized with Uint16.Max members if this is the first call.</param>
    ''' <returns>same as InitialDictionary just allowing you to nest it in the
    ''' ConstructConversionArray function</returns>
    Public Function ConstructInitialDictionary(ByVal InputData() As Byte, ByRef InitialDictionary() As DGroup) As DGroup()
        Dim Uint16val As UInt16
        Dim HalfVal As Half
        For ReadIndex As Integer = 0 To InputData.Length - 1 Step 2
            Uint16val = BitConverter.ToUInt16(InputData, ReadIndex)
            HalfVal = BitConverter.ToHalf(InputData, ReadIndex)

            If IsNothing(InitialDictionary(Uint16val)) Then
                Dim TempGroup As New DGroup
                TempGroup.value = HalfVal
                TempGroup.Uint16val = Uint16val
                InitialDictionary(Uint16val) = TempGroup
            End If
            InitialDictionary(Uint16val).TotalCount += 1UL
        Next
        Return InitialDictionary
    End Function
    ''' <summary>
    ''' Creates an array based mapping to convert floating point values to their most probable existing neighbors
    ''' weighted by the specified DistancePenalty to give closer values more priority.
    ''' </summary>
    ''' <param name="InitialDictionary"> Dictionary from ConstructInitialDictionary</param>
    ''' <param name="DistancePenalty">percentage of probability to subtract for each value per distance away from
    ''' the nearest existing neighbor. higher values favor proximity over probability and lower favors probability </param>
    ''' <returns>array based mapping to convert floating point values(bit converted to Uint16 and used as index values) 
    ''' to their most probable existing neighbors weighted by the specified DistancePenalty</returns>
    Public Function ConstructConversionArray(ByVal InitialDictionary() As DGroup, ByVal DistancePenalty As Double) As Half()
        Dim ConversionArray(UInt16.MaxValue) As Half
        Dim LastValue As DGroup = Nothing
        Dim x As UInt16
        While Not IsNothing(InitialDictionary(x))
            x += 1
        End While
        For FillCounter1 As Integer = 0 To x - 1
            ConversionArray(FillCounter1) = InitialDictionary(x).value
        Next
        LastValue = InitialDictionary(x)
        x += 1
        For DictionaryCounter As UInt16 = x To UInt16.MaxValue
            If Not IsNothing(InitialDictionary(DictionaryCounter)) Then
                ConversionArray(DictionaryCounter) = InitialDictionary(DictionaryCounter).value

                For FillCounter2 As Integer = LastValue.Uint16val + 1 To DictionaryCounter - 1
                    Dim PreviousValueDistancePenalty = (((FillCounter2 - LastValue.Uint16val) * DistancePenalty) * LastValue.TotalCount)
                    Dim CurrentValueDistancePenalty = (((DictionaryCounter - FillCounter2) * DistancePenalty) * InitialDictionary(DictionaryCounter).TotalCount)
                    If LastValue.TotalCount - PreviousValueDistancePenalty > InitialDictionary(DictionaryCounter).TotalCount Then
                        ConversionArray(FillCounter2) = LastValue.value
                    Else
                        ConversionArray(FillCounter2) = InitialDictionary(DictionaryCounter).value
                    End If
                Next
                LastValue = InitialDictionary(DictionaryCounter)
            End If
        Next
        Return ConversionArray
    End Function

    Public Class DGroup
        Public value As Half
        Public TotalCount As UInt64
        'highest and lowest are used during writing the file to expedite locating the correct group
        Public Uint16val As UInt16
    End Class

End Module
