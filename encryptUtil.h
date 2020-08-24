#include <string>
using namespace std;
int main();
void XOREncrypt(char output[], char key[], char plaintext[], int offset, int length);
void XOREncryptWithBufferAndThreads(string outputPath, string keyPath, string plainttextPath);
void XOREncryptWithBufferAndThreads(ofstream* output, char key[], ifstream* plaintext);
char* RotateKey(char key[], int x);
char* RotateKey(char key[]);
void CheckFilesSame(string filePath1, string filePath2);
