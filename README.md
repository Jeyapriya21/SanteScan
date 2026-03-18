# Santé Scan - Analyse de Bilans Sanguins par IA 🩺

**Santé Scan** est une application web Fullstack conçue pour simplifier la compréhension des analyses médicales. En combinant la puissance de l'OCR pour l'extraction de texte et l'Intelligence Artificielle locale (**Ollama**) pour l'interprétation, l'application offre aux utilisateurs un résumé clair et rapide de leur santé, tout en garantissant la confidentialité totale des données.

---

## Sommaire
- [Fonctionnalités](#fonctionnalités)
- [Technologies Utilisées](#technologies-utilisées)
- [Architecture](#architecture)
- [Installation Rapide](#installation-rapide)
- [Prochaines Évolutions](#prochaines-évolutions)

---

## Fonctionnalités

### 👤 Fonctionnalités Utilisateur
* **Analyse Instantanée (Mode Invité) :** Possibilité de scanner un document (PDF/Image) dès l'arrivée sur le site sans création de compte immédiate grâce à un système de session persistante.
* **Extraction OCR :** Reconnaissance automatique du texte à partir de photos (JPG/PNG) ou de fichiers PDF de bilans sanguins.
* **Interprétation par IA :** Génération d'un résumé intelligent expliquant les résultats de manière simplifiée via un modèle de langage local.
* **Historique des Bilans :** Une fois connecté, l'utilisateur peut consulter ses anciennes analyses et suivre l'évolution de ses marqueurs de santé.
* **Interface Intuitive :** Drag & Drop de fichiers et retour visuel en temps réel pendant l'analyse.

### 🛡️ Fonctionnalités Admin & Sécurité
* **Confidentialité Totale :** Utilisation d'Ollama (IA locale) pour que les données médicales sensibles ne quittent jamais le serveur.
* **Gestion des Sessions :** Utilisation de `X-Session-Id` pour lier des analyses anonymes à un compte utilisateur créé ultérieurement.
* **Validation de fichiers :** Contrôle strict de la taille (max 10MB) et du format des documents envoyés.

---

## Technologies Utilisées

### 💻 Frontend
* **Vue.js 3** (Composition API)
* **TypeScript** pour une logique typée et robuste
* **Tailwind CSS** pour un design moderne et responsive
* **Vite** comme outil de build

### ⚙️ Backend
* **ASP.NET Core 8** (C#)
* **Entity Framework Core** avec **SQLite**
* **OCR Service** pour le traitement d'images
* **Ollama API** pour l'intégration de modèles IA (Llama 3 / Mistral)
* **Authentification JWT** pour la sécurisation des comptes

---

## Architecture

Le projet suit les principes de la **Clean Architecture** pour garantir une maintenance facile et une séparation des responsabilités :
* **Models :** Entités métier (User, Analysis, BloodTestDetails).
* **Services :** Logique technique (OCR, Ollama, Session Management).
* **Controllers :** API REST exposant les points d'entrée (Endpoints) au Frontend.

---

## Installation Rapide

1.  **Pré-requis :**
    * .NET 8 SDK
    * Node.js (v18+)
    * [Ollama](https://ollama.com/) installé et lancé (`ollama run llama3`)

2.  **Configuration du Backend :**
    ```bash
    cd santeScan.Server
    dotnet ef database update
    dotnet run
    ```

3.  **Configuration du Frontend :**
    ```bash
    cd santescan.client
    npm install
    npm run dev
    ```

---

## Prochaines Évolutions (Incoming Features)

* **Visualisation Graphique :** Graphiques d'évolution pour comparer les résultats de plusieurs bilans dans le temps.
* **Export PDF :** Génération d'un rapport de synthèse propre à partager avec son médecin.
* **Google OAuth :** Authentification simplifiée via les comptes Google.
* **Multi-langues :** Support pour l'analyse de documents médicaux en plusieurs langues.

---

> **Note :** Ce projet est en cours de développement actif. L'objectif est de rendre la donnée médicale accessible à tous tout en respectant strictement la vie privée des utilisateurs.