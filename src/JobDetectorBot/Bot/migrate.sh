#!/bin/bash
set -e

# ����, ���� �� ����� ������
until pg_isready -h db -U postgres -d botdbcont; do
  echo "�������� ���������� PostgreSQL..."
  sleep 1
done

# ��������� ��������
dotnet ef database update

# ��������� �������� ����������
exec dotnet Bot.dll