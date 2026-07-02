#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

void Refract_float(float3 View, float3 Normal, float IOR, out float3 Out) {
	Out = refract(View,Normal,IOR);
}
#endif