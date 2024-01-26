﻿#include "pch-c.h"
#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include "codegen/il2cpp-codegen-metadata.h"





// 0x00000001 T Unity.Burst.Unsafe::Read(System.Void*)
// 0x00000002 T Unity.Burst.Unsafe::ReadUnaligned(System.Void*)
// 0x00000003 T Unity.Burst.Unsafe::ReadUnaligned(System.Byte&)
// 0x00000004 System.Void Unity.Burst.Unsafe::Write(System.Void*,T)
// 0x00000005 System.Void Unity.Burst.Unsafe::WriteUnaligned(System.Void*,T)
// 0x00000006 System.Void Unity.Burst.Unsafe::WriteUnaligned(System.Byte&,T)
// 0x00000007 System.Void Unity.Burst.Unsafe::Copy(System.Void*,T&)
// 0x00000008 System.Void Unity.Burst.Unsafe::Copy(T&,System.Void*)
// 0x00000009 System.Void* Unity.Burst.Unsafe::AsPointer(T&)
// 0x0000000A System.Int32 Unity.Burst.Unsafe::SizeOf()
// 0x0000000B System.Void Unity.Burst.Unsafe::CopyBlock(System.Void*,System.Void*,System.UInt32)
extern void Unsafe_CopyBlock_m275884E0E5269EE20C4FDB816A13EB0A33A5DD28 (void);
// 0x0000000C System.Void Unity.Burst.Unsafe::CopyBlock(System.Byte&,System.Byte&,System.UInt32)
extern void Unsafe_CopyBlock_mA8175DDFFADA01EFD26E7DC6C1056471691EEDC7 (void);
// 0x0000000D System.Void Unity.Burst.Unsafe::CopyBlockUnaligned(System.Void*,System.Void*,System.UInt32)
extern void Unsafe_CopyBlockUnaligned_mDC74221DAD22498BB43DABB5102F88D594774FEB (void);
// 0x0000000E System.Void Unity.Burst.Unsafe::CopyBlockUnaligned(System.Byte&,System.Byte&,System.UInt32)
extern void Unsafe_CopyBlockUnaligned_mD13DB0668E061153051CCE10829A29C5D8EB9509 (void);
// 0x0000000F System.Void Unity.Burst.Unsafe::InitBlock(System.Void*,System.Byte,System.UInt32)
extern void Unsafe_InitBlock_m965E8E4DCB55DB85E260C50DF5BCB4C1AC9E72D9 (void);
// 0x00000010 System.Void Unity.Burst.Unsafe::InitBlock(System.Byte&,System.Byte,System.UInt32)
extern void Unsafe_InitBlock_m22D44609198E040EFDD4B5FCC89DEF7C09EB420A (void);
// 0x00000011 System.Void Unity.Burst.Unsafe::InitBlockUnaligned(System.Void*,System.Byte,System.UInt32)
extern void Unsafe_InitBlockUnaligned_m59F5F56D7AF1385634EA2C8E6DA4ACFCDBA4783E (void);
// 0x00000012 System.Void Unity.Burst.Unsafe::InitBlockUnaligned(System.Byte&,System.Byte,System.UInt32)
extern void Unsafe_InitBlockUnaligned_m8FA427864FE9EB5A31AA9672F5C3572782B6D1EF (void);
// 0x00000013 T Unity.Burst.Unsafe::As(System.Object)
// 0x00000014 T& Unity.Burst.Unsafe::AsRef(System.Void*)
// 0x00000015 T& Unity.Burst.Unsafe::AsRef(T&)
// 0x00000016 TTo& Unity.Burst.Unsafe::As(TFrom&)
// 0x00000017 T& Unity.Burst.Unsafe::Unbox(System.Object)
// 0x00000018 T& Unity.Burst.Unsafe::Add(T&,System.Int32)
// 0x00000019 System.Void* Unity.Burst.Unsafe::Add(System.Void*,System.Int32)
// 0x0000001A T& Unity.Burst.Unsafe::Add(T&,System.IntPtr)
// 0x0000001B T& Unity.Burst.Unsafe::AddByteOffset(T&,System.IntPtr)
// 0x0000001C T& Unity.Burst.Unsafe::Subtract(T&,System.Int32)
// 0x0000001D System.Void* Unity.Burst.Unsafe::Subtract(System.Void*,System.Int32)
// 0x0000001E T& Unity.Burst.Unsafe::Subtract(T&,System.IntPtr)
// 0x0000001F T& Unity.Burst.Unsafe::SubtractByteOffset(T&,System.IntPtr)
// 0x00000020 System.IntPtr Unity.Burst.Unsafe::ByteOffset(T&,T&)
// 0x00000021 System.Boolean Unity.Burst.Unsafe::AreSame(T&,T&)
// 0x00000022 System.Boolean Unity.Burst.Unsafe::IsAddressGreaterThan(T&,T&)
// 0x00000023 System.Boolean Unity.Burst.Unsafe::IsAddressLessThan(T&,T&)
// 0x00000024 System.Void System.Runtime.Versioning.NonVersionableAttribute::.ctor()
extern void NonVersionableAttribute__ctor_mBC3056ECFEE16B17F3779A50EBDCCD519078E862 (void);
// 0x00000025 System.Void System.Runtime.CompilerServices.IsReadOnlyAttribute::.ctor()
extern void IsReadOnlyAttribute__ctor_m5CCFBC2EDEEA918A10A9C5A57A355234A495046D (void);
static Il2CppMethodPointer s_methodPointers[37] = 
{
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	Unsafe_CopyBlock_m275884E0E5269EE20C4FDB816A13EB0A33A5DD28,
	Unsafe_CopyBlock_mA8175DDFFADA01EFD26E7DC6C1056471691EEDC7,
	Unsafe_CopyBlockUnaligned_mDC74221DAD22498BB43DABB5102F88D594774FEB,
	Unsafe_CopyBlockUnaligned_mD13DB0668E061153051CCE10829A29C5D8EB9509,
	Unsafe_InitBlock_m965E8E4DCB55DB85E260C50DF5BCB4C1AC9E72D9,
	Unsafe_InitBlock_m22D44609198E040EFDD4B5FCC89DEF7C09EB420A,
	Unsafe_InitBlockUnaligned_m59F5F56D7AF1385634EA2C8E6DA4ACFCDBA4783E,
	Unsafe_InitBlockUnaligned_m8FA427864FE9EB5A31AA9672F5C3572782B6D1EF,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NULL,
	NonVersionableAttribute__ctor_mBC3056ECFEE16B17F3779A50EBDCCD519078E862,
	IsReadOnlyAttribute__ctor_m5CCFBC2EDEEA918A10A9C5A57A355234A495046D,
};
static const int32_t s_InvokerIndices[37] = 
{
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	10463,
	10463,
	10463,
	10463,
	10468,
	10468,
	10468,
	10468,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	0,
	7952,
	7952,
};
static const Il2CppTokenRangePair s_rgctxIndices[27] = 
{
	{ 0x06000001, { 0, 1 } },
	{ 0x06000002, { 1, 1 } },
	{ 0x06000003, { 2, 1 } },
	{ 0x06000004, { 3, 1 } },
	{ 0x06000005, { 4, 1 } },
	{ 0x06000006, { 5, 1 } },
	{ 0x06000007, { 6, 2 } },
	{ 0x06000008, { 8, 2 } },
	{ 0x06000009, { 10, 1 } },
	{ 0x0600000A, { 11, 1 } },
	{ 0x06000013, { 12, 1 } },
	{ 0x06000014, { 13, 1 } },
	{ 0x06000015, { 14, 1 } },
	{ 0x06000016, { 15, 2 } },
	{ 0x06000017, { 17, 2 } },
	{ 0x06000018, { 19, 2 } },
	{ 0x06000019, { 21, 1 } },
	{ 0x0600001A, { 22, 2 } },
	{ 0x0600001B, { 24, 1 } },
	{ 0x0600001C, { 25, 2 } },
	{ 0x0600001D, { 27, 1 } },
	{ 0x0600001E, { 28, 2 } },
	{ 0x0600001F, { 30, 1 } },
	{ 0x06000020, { 31, 1 } },
	{ 0x06000021, { 32, 1 } },
	{ 0x06000022, { 33, 1 } },
	{ 0x06000023, { 34, 1 } },
};
extern const uint32_t g_rgctx_T_tDD877A3959A32CDF3A18DC50B387BBA6605A3F66;
extern const uint32_t g_rgctx_T_t03FB86963CF767AE5231D7B25FD7B59B002FBC29;
extern const uint32_t g_rgctx_T_t4D1ACFEF06421BEE6BE1077F4D1159F041C27F40;
extern const uint32_t g_rgctx_T_tDD33C94856FA515E657F8D4557F7A7F88276F94E;
extern const uint32_t g_rgctx_T_t0860C0874D3F0A839E920D4817401DCFA437A6AE;
extern const uint32_t g_rgctx_T_tE62B899A6635624822BE50B42DC5AEFB6D80D6EF;
extern const uint32_t g_rgctx_TU26_t06AC09FCED1C575C931CB142B74E8443D382CCFC;
extern const uint32_t g_rgctx_T_tED6ED342D5C5717C25BF04E5D5673FDED01C94DA;
extern const uint32_t g_rgctx_TU26_tDEBCD8C3A477829576FC64A92BE9265D84CF893C;
extern const uint32_t g_rgctx_T_tB5DD24C5B45F3BA10033BD16141917BF2B7CF962;
extern const uint32_t g_rgctx_TU26_t5868BDE58819716E6F5DF8D910B7906B8E302A5E;
extern const uint32_t g_rgctx_T_tEB8D46F97245B882F63326D53230C79015902431;
extern const uint32_t g_rgctx_T_t19D04D2D5E9E91F3D528CF5397FB9134CAD0C1AF;
extern const uint32_t g_rgctx_TU26_t0EBEABF1318AF8F89B1E984CD9F1106408C57E95;
extern const uint32_t g_rgctx_TU26_t60B64F3F1ADDC2D5C9DC3487956093D8F33B3E1C;
extern const uint32_t g_rgctx_TFromU26_t19675E1684D51DF9FFFC463F33F13F9F3BE7E4DA;
extern const uint32_t g_rgctx_TToU26_t284EE66746491287ADAAAC2C632D932581B6F5B7;
extern const uint32_t g_rgctx_T_tF8135FE874E4C4B240ED271807B466F898DA8611;
extern const uint32_t g_rgctx_TU26_t60E1099EFE00D57EEDD8D47B68569D24C35FBD77;
extern const uint32_t g_rgctx_TU26_t25E3DBAA5C1334999F73FC235F5F23ABCDFED6CE;
extern const uint32_t g_rgctx_T_t027756B82C233E462AF43A21E66F07D4CB86FFAC;
extern const uint32_t g_rgctx_T_t905589086EA368E81EC0EEA886D17179F42579D0;
extern const uint32_t g_rgctx_TU26_t35E2C1228D07F3772A8965EC4568AE2A2F183FD5;
extern const uint32_t g_rgctx_T_t06D93A8452704F60EACE7C67DC38480527E1F631;
extern const uint32_t g_rgctx_TU26_tC1566EF32454214E44482A166EEED05392CDAF08;
extern const uint32_t g_rgctx_TU26_tE199FFB8B6AF4A804785D49BE7DE664170FE5B5E;
extern const uint32_t g_rgctx_T_t15FA8690674B2D208A84A61A2CB89EC6FEB61625;
extern const uint32_t g_rgctx_T_tB526FC7BA2695990ECCBF409C7D9E387F74A8B4A;
extern const uint32_t g_rgctx_TU26_t3F2BF895645F2CB162A5FB36086B661505566351;
extern const uint32_t g_rgctx_T_t19052742042FCDC19A9AF1289F3BD70DFE6C64D2;
extern const uint32_t g_rgctx_TU26_t0ED05D603F561EF61F2765F3ADA6D4BAE40B5DA2;
extern const uint32_t g_rgctx_TU26_t54976ECD3689673C3E656292E2357CF8640E946B;
extern const uint32_t g_rgctx_TU26_t24680731CAD469816431FAC5B96F1F3582FB7E44;
extern const uint32_t g_rgctx_TU26_t975D230C8BE1EE9CD37BD2314AE34E619AF49415;
extern const uint32_t g_rgctx_TU26_t7882A5BCEBD279C923124EC6C2D18614726EB47E;
static const Il2CppRGCTXDefinition s_rgctxValues[35] = 
{
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_tDD877A3959A32CDF3A18DC50B387BBA6605A3F66 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_t03FB86963CF767AE5231D7B25FD7B59B002FBC29 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_t4D1ACFEF06421BEE6BE1077F4D1159F041C27F40 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_tDD33C94856FA515E657F8D4557F7A7F88276F94E },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_t0860C0874D3F0A839E920D4817401DCFA437A6AE },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_tE62B899A6635624822BE50B42DC5AEFB6D80D6EF },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t06AC09FCED1C575C931CB142B74E8443D382CCFC },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_tED6ED342D5C5717C25BF04E5D5673FDED01C94DA },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_tDEBCD8C3A477829576FC64A92BE9265D84CF893C },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_tB5DD24C5B45F3BA10033BD16141917BF2B7CF962 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t5868BDE58819716E6F5DF8D910B7906B8E302A5E },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_tEB8D46F97245B882F63326D53230C79015902431 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_t19D04D2D5E9E91F3D528CF5397FB9134CAD0C1AF },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t0EBEABF1318AF8F89B1E984CD9F1106408C57E95 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t60B64F3F1ADDC2D5C9DC3487956093D8F33B3E1C },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TFromU26_t19675E1684D51DF9FFFC463F33F13F9F3BE7E4DA },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TToU26_t284EE66746491287ADAAAC2C632D932581B6F5B7 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_tF8135FE874E4C4B240ED271807B466F898DA8611 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t60E1099EFE00D57EEDD8D47B68569D24C35FBD77 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t25E3DBAA5C1334999F73FC235F5F23ABCDFED6CE },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_t027756B82C233E462AF43A21E66F07D4CB86FFAC },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_t905589086EA368E81EC0EEA886D17179F42579D0 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t35E2C1228D07F3772A8965EC4568AE2A2F183FD5 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_t06D93A8452704F60EACE7C67DC38480527E1F631 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_tC1566EF32454214E44482A166EEED05392CDAF08 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_tE199FFB8B6AF4A804785D49BE7DE664170FE5B5E },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_t15FA8690674B2D208A84A61A2CB89EC6FEB61625 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_tB526FC7BA2695990ECCBF409C7D9E387F74A8B4A },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t3F2BF895645F2CB162A5FB36086B661505566351 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_T_t19052742042FCDC19A9AF1289F3BD70DFE6C64D2 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t0ED05D603F561EF61F2765F3ADA6D4BAE40B5DA2 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t54976ECD3689673C3E656292E2357CF8640E946B },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t24680731CAD469816431FAC5B96F1F3582FB7E44 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t975D230C8BE1EE9CD37BD2314AE34E619AF49415 },
	{ (Il2CppRGCTXDataType)2, (const void *)&g_rgctx_TU26_t7882A5BCEBD279C923124EC6C2D18614726EB47E },
};
IL2CPP_EXTERN_C const Il2CppCodeGenModule g_Unity_Burst_Unsafe_CodeGenModule;
const Il2CppCodeGenModule g_Unity_Burst_Unsafe_CodeGenModule = 
{
	"Unity.Burst.Unsafe.dll",
	37,
	s_methodPointers,
	0,
	NULL,
	s_InvokerIndices,
	0,
	NULL,
	27,
	s_rgctxIndices,
	35,
	s_rgctxValues,
	NULL,
	NULL, // module initializer,
	NULL,
	NULL,
	NULL,
};
