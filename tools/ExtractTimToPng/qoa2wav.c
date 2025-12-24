#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>

#define QOA_IMPLEMENTATION
#include "../../wipeout-rewrite/src/libs/qoa.h"

// WAV file header structure
typedef struct {
    char riff[4];           // "RIFF"
    uint32_t file_size;     // File size - 8
    char wave[4];           // "WAVE"
    char fmt[4];            // "fmt "
    uint32_t fmt_size;      // 16 for PCM
    uint16_t audio_format;  // 1 for PCM
    uint16_t num_channels;
    uint32_t sample_rate;
    uint32_t byte_rate;
    uint16_t block_align;
    uint16_t bits_per_sample;
    char data[4];           // "data"
    uint32_t data_size;
} wav_header_t;

void write_wav_header(FILE *fp, uint32_t sample_rate, uint16_t channels, uint32_t num_samples) {
    wav_header_t header;
    uint32_t data_size = num_samples * channels * 2; // 16-bit samples
    
    memcpy(header.riff, "RIFF", 4);
    header.file_size = data_size + 36;
    memcpy(header.wave, "WAVE", 4);
    memcpy(header.fmt, "fmt ", 4);
    header.fmt_size = 16;
    header.audio_format = 1;
    header.num_channels = channels;
    header.sample_rate = sample_rate;
    header.byte_rate = sample_rate * channels * 2;
    header.block_align = channels * 2;
    header.bits_per_sample = 16;
    memcpy(header.data, "data", 4);
    header.data_size = data_size;
    
    fwrite(&header, sizeof(wav_header_t), 1, fp);
}

int convert_qoa_to_wav(const char *qoa_path, const char *wav_path) {
    FILE *qoa_file = fopen(qoa_path, "rb");
    if (!qoa_file) {
        fprintf(stderr, "Error: Cannot open QOA file: %s\n", qoa_path);
        return 1;
    }
    
    // Get file size
    fseek(qoa_file, 0, SEEK_END);
    long file_size = ftell(qoa_file);
    fseek(qoa_file, 0, SEEK_SET);
    
    // Read entire QOA file
    void *qoa_data = malloc(file_size);
    fread(qoa_data, 1, file_size, qoa_file);
    fclose(qoa_file);
    
    // Decode QOA
    qoa_desc qoa;
    short *sample_data = qoa_decode(qoa_data, (int)file_size, &qoa);
    free(qoa_data);
    
    if (!sample_data) {
        fprintf(stderr, "Error: Failed to decode QOA file\n");
        return 1;
    }
    
    printf("Decoded: %u samples, %u channels, %u Hz\n", 
           qoa.samples, qoa.channels, qoa.samplerate);
    
    // Write WAV file
    FILE *wav_file = fopen(wav_path, "wb");
    if (!wav_file) {
        fprintf(stderr, "Error: Cannot create WAV file: %s\n", wav_path);
        free(sample_data);
        return 1;
    }
    
    write_wav_header(wav_file, qoa.samplerate, qoa.channels, qoa.samples * qoa.channels);
    fwrite(sample_data, sizeof(short), qoa.samples * qoa.channels, wav_file);
    fclose(wav_file);
    free(sample_data);
    
    printf("✓ Converted: %s -> %s\n", qoa_path, wav_path);
    return 0;
}

int main(int argc, char *argv[]) {
    if (argc < 2) {
        printf("QOA to WAV Converter\n");
        printf("Usage: %s <input.qoa> [output.wav]\n", argv[0]);
        printf("   or: %s <directory>\n", argv[0]);
        return 1;
    }
    
    // Check if argument is a directory
    FILE *test = fopen(argv[1], "rb");
    if (!test) {
        fprintf(stderr, "Error: Cannot access: %s\n", argv[1]);
        return 1;
    }
    fclose(test);
    
    // Single file conversion
    char wav_path[512];
    if (argc >= 3) {
        strcpy(wav_path, argv[2]);
    } else {
        strcpy(wav_path, argv[1]);
        char *ext = strrchr(wav_path, '.');
        if (ext) {
            strcpy(ext, ".wav");
        } else {
            strcat(wav_path, ".wav");
        }
    }
    
    return convert_qoa_to_wav(argv[1], wav_path);
}
