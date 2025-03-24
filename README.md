This is a side project of mine attempting to improving accuracy of other lossy compression like quantization not compressing the data it's self. 
This is untested alpha software proceed accordingly(it does at least compile and had Gemini 12B look for issues but found nothing major). This targets 
FP16 sources, FP32 would take a lot of memory which might kill it's benefit. I am just posting this as a toy for someone to play with to see if it is 
beneficial or not.
Note:FileIO is left to the user.
Step 1.Create an array of Dgroup with enough elements to hold the full unsigned 16 bit integer range this will be your intial dictionary. 
Step 2.run ConstructInitialDictionary on the FP16 of the data you are going to quantize and the Dgroup array to create a frequency distribution.
Step 3.optionally repeat step 1 for each additional chunk of data and the dgroup array from step 2 creating a dictionary representing all of them. 
Step 4.pass the dictionary from step 2/3 to ConstructConversionArray along with a distance penalty and it will produce an array of half values. 
Note:Distance Penalty is applied per value away from the relevant neighboring value as a percentage subtracted from the percentage chance the
neighbor appears in the original.
Step 5.when you decode a quantized file you can bit convert the results to unsigned 16 bit integers and use them as an index in the array from
step 4 to get the most probable neighbor based on your distance penalty and the frequency data.

Note: something some may find confusing is the use of 16bit floats converted to 16 bit unsigned integers. this is primarily to allow for direct
array indexing and distance calculations so I didn't have to deal with the variable distance between floats. if this causes problems could probably
replace with a something else to skip the conversions at a higher memory cost but it would also likely be a little more complicated due to having to deal
with non-finite values.
