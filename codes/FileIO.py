# coding=utf-8

def SortFile(srcFile, destFile):
    reader = open(srcFile, 'a+', 1, 'utf-8')
    reader.seek(0)
    #print(reader.readline())
    #print(reader.read())

    writer = open(destFile, 'w+', 1, 'utf-8')
    for line in reader.readlines():
        ls = line.split('\t')
        writer.write(ls[0] + '\tx\t' + ls[1])

    reader.close()
    writer.close()


if __name__ == "__main__":
    SortFile("C:\\Users\\yajxu\\Desktop\\test\\Cat\\Injection\\Cat3.TestCase.txt", "C:\\Users\\yajxu\\Desktop\\test.txt")