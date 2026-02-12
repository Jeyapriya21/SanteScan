<template>
  <div class="w-full">
    <input 
      type="file" 
      ref="fileInput" 
      @change="handleFileUpload" 
      accept="image/*,.pdf" 
      class="hidden" 
    />

    <section 
      @click="triggerFileInput"
      @dragover.prevent="onDragOver" 
      @dragleave.prevent="onDragLeave" 
      @drop.prevent="onDrop"
      :class="[
        'relative cursor-pointer border-2 border-dashed rounded-3xl p-10 transition-all duration-300 flex flex-col items-center justify-center gap-6 group',
        isDragging 
          ? 'border-blue-500 bg-blue-50 scale-[1.01] shadow-xl' 
          : 'border-slate-300 bg-white hover:border-blue-400 hover:bg-slate-50'
      ]"
    >
      <div v-if="!isUploading" class="text-5xl group-hover:scale-110 transition-transform duration-300">
        {{ isDragging ? '📂' : '📄' }}
      </div>

      <div class="w-full max-w-xs bg-sky-400 text-slate-900 font-bold py-4 rounded-xl shadow-[0_4px_0_0_rgba(14,165,233,1)] border-2 border-slate-800 flex items-center justify-center gap-3 group-hover:bg-sky-500">
        <span v-if="!isUploading" class="text-2xl">📷</span>
        <span v-else class="animate-spin text-2xl">⏳</span>
        <span class="text-lg uppercase italic tracking-wide">
          {{ isUploading ? 'Analyse IA...' : 'Scanner mon bilan' }}
        </span>
      </div>

      <div class="text-center">
        <p class="text-slate-500 font-bold italic">
          {{ isDragging ? 'Relâchez pour analyser' : 'Cliquez ou glissez votre bilan ici' }}
        </p>
      </div>
    </section>

    <p v-if="uploadError" class="text-center text-sm text-red-500 mt-4 font-bold p-2 bg-red-50 rounded-lg">
      ⚠️ {{ uploadError }}
    </p>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

// 1. Définition de l'interface pour le résultat de l'IA
interface AnalysisResult {
  date: string;
  status: string;
  note: string;
}

// 2. Définition des événements que le composant peut envoyer
const emit = defineEmits<{
  (e: 'analysis-finished', result: AnalysisResult): void;
}>();

const fileInput = ref<HTMLInputElement | null>(null);
const isUploading = ref<boolean>(false);
const isDragging = ref<boolean>(false);
const uploadError = ref<string | null>(null);

const triggerFileInput = (): void => {
  fileInput.value?.click();
};

const onDragOver = (): void => { isDragging.value = true; };
const onDragLeave = (): void => { isDragging.value = false; };

const onDrop = (event: DragEvent): void => {
  isDragging.value = false;
  const file = event.dataTransfer?.files[0];
  if (file) processFile(file);
};

const handleFileUpload = (event: Event): void => {
  const target = event.target as HTMLInputElement;
  const file = target.files?.[0];
  if (file) processFile(file);
};

const processFile = async (file: File): Promise<void> => {
  const validTypes: string[] = ['image/jpeg', 'image/png', 'application/pdf'];
  
  if (!validTypes.includes(file.type)) {
    uploadError.value = "Format non supporté (PDF ou Image uniquement).";
    return;
  }

  isUploading.value = true;
  uploadError.value = null;

  const formData = new FormData();
  formData.append('file', file);

  try {
    const response = await fetch('http://localhost:5000/api/scan', {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) throw new Error("Erreur lors de l'analyse IA");

    const data: AnalysisResult = await response.json();
    
    // On envoie le résultat typé au parent
    emit('analysis-finished', data);

  } catch (err) {
    uploadError.value = err instanceof Error ? err.message : "Erreur inconnue";
  } finally {
    isUploading.value = false;
  }
};
</script>