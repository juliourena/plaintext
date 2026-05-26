# ~/.config/ai-local/ollama-host.env
# Ollama remoto: host desde la VM
export OLLAMA_HOST="http://xxxxx:11434"

# Claude Code usando Ollama Anthropic-compatible API
export ANTHROPIC_AUTH_TOKEN="ollama"
export ANTHROPIC_API_KEY=""
export ANTHROPIC_BASE_URL="htp://xxxx:11434"

# Modelo para Claude Code
export CLAUDE_MODEL="gpt-oss:20b"


# ~/bin/claude-ollama
# chmod +x ~/bin/claude-ollama
# echo 'export PATH="$HOME/bin:$PATH"' >> ~/.bashrc
# source ~/.bashrc

#!/usr/bin/env bash
set -e

source ~/.config/ai-local/ollama-host.env

exec claude --model "$CLAUDE_MODEL" "$@"
