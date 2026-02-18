<template>
  <div class="w-full">
    <input type="file"
           ref="fileInput"
           @change="handleFileUpload"
           accept="image/*,.pdf"
           class="hidden" />

    <section @click="triggerFileInput"
             @dragover.prevent="onDragOver"
             @dragleave.prevent="onDragLeave"
             @drop.prevent="onDrop"
             :class="[
        'relative cursor-pointer border-2 border-dashed rounded-3xl p-10 transition-all duration-300 flex flex-col items-center justify-center gap-6 group',
        isDragging
          ? 'border-blue-500 bg-blue-50 scale-[1.01] shadow-xl'
          : 'border-slate-300 bg-white hover:border-blue-400 hover:bg-slate-50'
      ]">
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

  interface AnalysisResult {
    analysisId: string;
    message: string;
    uploadDate: string;
    status: string;
  }

  const emit = defineEmits<{
    (e: 'analysis-finished', result: AnalysisResult): void;
  }>();

  const fileInput = ref<HTMLInputElement | null>(null);
  const isUploading = ref<boolean>(false);
  const isDragging = ref<boolean>(false);
  const uploadError = ref<string | null>(null);

  // ✅ NOUVEAU : Fonction pour générer/récupérer un sessionId
  function getOrCreateSessionId(): string {
    const STORAGE_KEY = 'santescan_session_id';
    let sessionId = localStorage.getItem(STORAGE_KEY);

    if (!sessionId) {
      // Générer un UUID v4
      sessionId = crypto.randomUUID();
      localStorage.setItem(STORAGE_KEY, sessionId);
      console.log('✅ Nouveau sessionId créé:', sessionId);
    } else {
      console.log('📌 SessionId existant:', sessionId);
    }

    return sessionId;
  }

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
    const validTypes: string[] = ['image/jpeg', 'image/jpg', 'image/png', 'application/pdf'];
    const validExtensions: string[] = ['.jpg', '.jpeg', '.png', '.pdf'];
    const fileExtension = '.' + file.name.split('.').pop()?.toLowerCase();

    if (!validTypes.includes(file.type) || !validExtensions.includes(fileExtension)) {
      uploadError.value = "Format non supporté. Formats acceptés : JPG, PNG, PDF";
      return;
    }

    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
      uploadError.value = "Fichier trop volumineux (max 10 MB)";
      return;
    }

    isUploading.value = true;
    uploadError.value = null;

    const formData = new FormData();
    formData.append('file', file);

    // ✅ CORRECTION : Obtenir le sessionId
    const sessionId = getOrCreateSessionId();
    const token = localStorage.getItem('user_token');

    try {
    const response = await fetch('/api/Analyses/upload', {
      method: 'POST',
      headers: {
        // ✅ On envoie le Session ID (Requis par ton nouveau contrôleur)
        'X-Session-Id': sessionId,
        // ✅ On envoie le Token s'il existe
        ...(token && { 'Authorization': `Bearer ${token}` })
      },
      body: formData,
    });

    if (!response.ok) {
      // Si erreur 500, on essaie de lire le JSON pour voir le message d'erreur
      const errorText = await response.text();
      console.error("Détails de l'erreur serveur:", errorText);
      throw new Error(`Erreur serveur (${response.status})`);
    }

    const result = await response.json();
    emit('analysis-finished', result);

  } catch (err: any) {
    uploadError.value = err.message || "Erreur lors de l'envoi";
    console.error("❌ Erreur complète:", err);
  } finally {
    isUploading.value = false;
  }
  };
</script>
