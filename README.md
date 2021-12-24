# Program Language Test

## Info
Assembly and batch file structured programming language, uses strong types
Some parts are not consistent, this was an experiment, not for real use

## Sample codes
prglng\bin\Debug\

## Usage
prglng.exe [filename]
For ex. prglng.exe first.ini

## Keywords
Some may be incorrect, since it was never documented
```
! - Comment
:label - Jump label

DEF [var] [type] - Define variable
UDF [var] - Undefine variable

ARA [var] [size] - Initialize array
ARS [var] [index]- Set array index
ARG [var] [index] - Set array index
MOV [var] [value] - Set variable

CMP [var1] [var2] [type] [EQ/NQ/GR/GE/LS/LE] [label] [EJ - else jump] - Compare then jump to label
JMP [label] - Jump to label

EQ - Equal
NQ - Not equal
GR - Greater
GE - Greater and equal
LS - Less
LE - Less and equal

INS [var] [type](par1,par2...) [par1 type] [par2 type]... - Instantiate
IET [var] [method](par1,par2...) [par1 type] [par2 type]... - Instance call without getting return value
GET [var1] [var2] [method](par1,par2...) [par1 type] [par2 type]... - Instance call with getting return value
GET [var] [type] [method](par1,par2...) [par1 type] [par2 type]... - Static call with getting return value
MET [type] [method](par1,par2...) [par1 type] [par2 type]... - Static call without getting return value

ADD [var1] [var2] [type] - Add variable
SUB [var1] [var2] [type] - Subtract variable
MUL [var1] [var2] [type] - Multiply variable
DIV [var1] [var2] [type] - Divide variable
MOD [var1] [var2] [type] - Divide remainder variable
LOR [var1] [var2] [type] - Logical or variable
AND [var1] [var2] [type] - Logical and variable
XOR [var1] [var2] [type] - Logical xor variable
ROL [var1] [var2] [type] - Byteshift left variable
ROR [var1] [var2] [type] - Byteshift right variable

Used in function calls and CMP keyword
% - value type variable
& - reference type variable
```
## Tools Needed To load the project properly
- Visual C# 2008 Express or newer
