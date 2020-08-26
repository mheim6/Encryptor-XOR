# encryptUtil

MONICA HEIM 
-----------------------------------

THIS CODE RUNS AND COMPILES. 


In this Coding Assignment I decided to program it in 2 languages. C# and C++.
I created a utility that would perform a simple XOR cryptographic transform on a given set of data - an XOR stream encryptor.

The encryption key in my program is called "keyONEMB.txt". The size of the key in bytes will dictate the "block size." 

The plaintext data can be found as "plaintextONEGB.txt", where the utility will then break it into block-sized sections, XOR it against the key, and write the cypher text to another file called "resultONEGB.txt".

After each block is processed, the key will rotate to the left by one bit to make a new key. This means the key will repeat every N blocks, where N is the number of bits in the key. 

The plaintext data is not a multiple of the block size in length, nor is it assumed to be ASCII or Unicode text. 

In my code is also valid for the plaintext to be extremely large, far exceeding the available memory+swap space for the system.

Finally I did some unit testing on my code to make sure it was working correct
"static void Main(string[] args)
        {
            UnitTest("f0f0", "0102030411121314", "f1f2e2e5d2d19493");
        }"



For better results please:

C++: Open encryptUtil.cpp and run it. (Make sure that the .h and .txt files are in the same folder)

C#: Open encryptUtil.cs and run it.   (Make sure that the .txt files are in the same folder)
