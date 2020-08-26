//Monica Heim
//August 23,2020

#include <chrono>
#include <vector>
#include <iostream>
#include <cstdlib>
#include <string>
#include <fstream>
#include <thread>
#include <experimental/filesystem>
#include <ostream>
#include <sstream>
#include "encryptUtil.h"
using namespace std;

static const int THREAD_COUNT = 8;
static const int BUFFER_SIZE = 10000000 / THREAD_COUNT;
static const int KEY_LENGTH = 1048576;
static char key[KEY_LENGTH];
static char inputBuffers[THREAD_COUNT][BUFFER_SIZE];
static char outputBuffers[THREAD_COUNT][BUFFER_SIZE];

int main()
{
	std::cout << "Encrypting File (Please Wait)\n";

	XOREncryptWithBufferAndThreads("resultONEGB.txt", "keyONEMB.txt", "plaintextONEGB.txt");
	CheckFilesSame("resultONEGB.txt", "resultONEGBGood.txt");
	std::cout << "Encryption Done\n";

}

void XOREncrypt(char output[],  char key[], char plaintext[], int offset, int length)
{
	for (int i = 0; i < length; i++)
	{
		output[i] = (char)(plaintext[i] ^ key[(i + offset) % KEY_LENGTH]);
		if ((i + offset) % KEY_LENGTH == KEY_LENGTH - 1)
		{
			key = RotateKey(key);
		}
	}
}

void XOREncryptWithBufferAndThreads(string outputPath, string keyPath, string plainttextPath)
{
	ifstream keyReader;
	keyReader.open(keyPath);
	keyReader.read(key, KEY_LENGTH);
	keyReader.close();
	ifstream reader;
	ofstream writer;
	reader.open(plainttextPath);
	writer.open(outputPath);
	XOREncryptWithBufferAndThreads(&writer, key, &reader);
	reader.close();
	writer.close();
}

void XOREncryptWithBufferAndThreads(ofstream* output, char key[], ifstream* plaintext)
{
	int plaintextLength = 1073741824;
	int numFullBlocks = (int)(plaintextLength / BUFFER_SIZE);
	int numExtra = (int)(plaintextLength % BUFFER_SIZE);
	int numThreadBlocks = (numFullBlocks + THREAD_COUNT - 1) / THREAD_COUNT;

	for (int i = 0; i < numThreadBlocks; i++)
	{
		int numThreads = i == numThreadBlocks - 1 ? numFullBlocks % THREAD_COUNT : THREAD_COUNT;
		for (int j = 0; j < numThreads; j++) (*plaintext).read(inputBuffers[j], BUFFER_SIZE);
		for (int x = 0; x < numThreads; x++)
		{
			char* thisKey = RotateKey(key, (i * BUFFER_SIZE * THREAD_COUNT + x) / KEY_LENGTH);

			XOREncrypt(outputBuffers[x], thisKey, inputBuffers[x], i * BUFFER_SIZE * THREAD_COUNT + x, BUFFER_SIZE);
		}
		for (int j = 0; j < numThreads; j++) (*output).write(outputBuffers[j], BUFFER_SIZE);
	}
	if (numExtra > 0)
	{
		(*plaintext).read(inputBuffers[0], numExtra);
		char* thisKey = RotateKey(key, (numFullBlocks * BUFFER_SIZE) / KEY_LENGTH);
		XOREncrypt(outputBuffers[0], thisKey, inputBuffers[0], numFullBlocks * BUFFER_SIZE, numExtra);
		(*output).write(outputBuffers[0], numExtra);
	}
}

char* RotateKey(char key[], int x)
{
	char newKey[KEY_LENGTH];
	for (int i = 0; i < KEY_LENGTH; i++)
	{
		char prevByte = key[(i + x / 8) % KEY_LENGTH];
		char nextByte = key[(i + x / 8 + 1) % KEY_LENGTH];
		newKey[i] = (char)(((unsigned char) prevByte << (x % 8)) + ((unsigned char)nextByte >> (8 - x % 8)));
	}
	return newKey;
}

char* RotateKey(char key[])
{
	// rotate to the left by one bit
	char newKey[KEY_LENGTH];
	for (int i = 0; i < KEY_LENGTH; i++)
	{
		char nextByte = key[(i + 1) % KEY_LENGTH];
		newKey[i] = (char)((((unsigned char)key[i]) << 1) + (((unsigned char)nextByte) >> 7));
	}
	return newKey;
}

void CheckFilesSame(string filePath1, string filePath2)
{
	char compareBuffer1[1024];
	char compareBuffer2[1024];
	ifstream reader1;
	reader1.open(filePath1);
	reader1.read(compareBuffer1, 1024);
	reader1.close();
	ifstream reader2;
	reader2.open(filePath2);
	reader2.read(compareBuffer2, 1024);
	reader2.close();

	for (int i = 0; i < 1024; i++)
	{
		if (compareBuffer1[i] != compareBuffer2[i]) {
			throw 20;
		}
	}
}
